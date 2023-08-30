using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasm.Sdk.Analyzer.Extensions;
using Wasm.Sdk.Analyzer.Generator;
using Wasm.Sdk.Analyzer.Proxy;
using Wasm.Sdk.Analyzer.Models;
using Wasm.Sdk.Analyzer.Resolvers;

namespace Wasm.Sdk.Analyzer
{
    class ModelProvider
    {
        private readonly Compilation compilation;
        private readonly TypeModelResolver typeModelResolver;
        private readonly IReadOnlyList<INamedTypeSymbol> arrayLikeTypes;
        private readonly INamedTypeSymbol voidType;

        public ModelProvider(Compilation compilation, TypeModelResolver typeModelResolver)
        {
            this.compilation = compilation;
            this.typeModelResolver = typeModelResolver;
            arrayLikeTypes = new List<INamedTypeSymbol>
            {
                compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1"),
            };
            voidType = compilation.GetTypeByMetadataName("System.Void");
        }

        private bool IsSupportedGenericPrimitive(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.TypeArguments.Count() == 1 && Constants.SupportedInteropTypes.Contains(typeSymbol.TypeArguments.First().ToString());
        }

        private bool IsArrayLikeSerializedType(ITypeSymbol typeSymbol, out string suggestedArrayType)
        {
            suggestedArrayType = null;
            var isArrayLikeSerializedType = false;
            if (typeSymbol is INamedTypeSymbol namedSymbol)
            {
                if (IsSupportedGenericPrimitive(namedSymbol) && arrayLikeTypes.Any(t => namedSymbol.IsDerivedFromClassOrInterface(t)))
                {
                    suggestedArrayType = namedSymbol.TypeArguments.First().ToString();
                    isArrayLikeSerializedType = true;
                }
            }

            return isArrayLikeSerializedType;
        }

        private IEnumerable<ParameterModel> GetParameterModels(IGeneratorContext context, SemanticModel model, ParameterListSyntax parameterListSyntax)
        {
            foreach (var parameter in parameterListSyntax.Parameters)
            {
                var paramSymbol = model.GetDeclaredSymbol(parameter);
                if (typeModelResolver.IsTaskType(paramSymbol.Type))
                {
                    context.Diagnostics.ReportDiagnostic(WasmDiagnostic.UnsupportedParameterType, parameter.GetLocation(), paramSymbol.Type.Name);
                    yield break;
                }

                // TODO perform additional checks if paramSymbol is ref etc.

                var symbolType = typeModelResolver.GetTypeModel(context, parameter.GetLocation(), paramSymbol.Type, typedArray: true);
                if (symbolType == null)
                {
                    yield break;
                }
                if (symbolType.WasmType.Nullable)
                {
                    context.Diagnostics.ReportDiagnostic(WasmDiagnostic.UnsupportedParameterType, parameter.GetLocation(), paramSymbol.Type.Name);
                }

                yield return new ParameterModel(
                    parameter,
                    symbolType,
                    new WasmParameter(
                        paramSymbol.Name, 
                        symbolType.WasmType
                    )
                );
            }
        }

        private IEnumerable<ConstructorModel> GetConstructorModels(SemanticModel model, ClassDeclarationSyntax classDeclaration, IGeneratorContext context)
        {
            var constructorDefinitions = classDeclaration
                .ChildNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .ToList();

            var publicConstructorDefinitions = constructorDefinitions
                .Where(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                .ToList();

            if (constructorDefinitions.Count == 0)
            {
                // Default ctor
                yield return new ConstructorModel(new WasmConstructor(Enumerable.Empty<WasmParameter>()), Enumerable.Empty<ParameterModel>());
            }
            else if (publicConstructorDefinitions.Count == 0)
            {
                context.Diagnostics.ReportDiagnostic(WasmDiagnostic.ClassHasNoPublicConstructor, classDeclaration.GetLocation());
            }
            else
            {
                foreach (var ctor in publicConstructorDefinitions)
                {
                    var constructorParameters = GetParameterModels(context, model, ctor.ParameterList);
                    if (constructorParameters != null)
                    {
                        yield return new ConstructorModel(new WasmConstructor(constructorParameters.Select(cp => cp.Parameter)), constructorParameters);
                    }
                }
            }
        }

        private IEnumerable<MethodModel> GetMethodModels(SemanticModel model, ClassDeclarationSyntax classDeclaration, IGeneratorContext context)
        {
            List<string> methodNames = new List<string>();

            var methodDefinitions = classDeclaration
                .ChildNodes()
                .OfType<MethodDeclarationSyntax>()
                .ToList();

            var publicMethodDefinitions = methodDefinitions
                .Where(m => m.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                .ToList();

            if (methodDefinitions.Count == 0)
            {
                context.Diagnostics.ReportDiagnostic(WasmDiagnostic.ClassHasNoMethods, classDeclaration.GetLocation());
            }
            else if (publicMethodDefinitions.Count == 0)
            {
                context.Diagnostics.ReportDiagnostic(WasmDiagnostic.ClassHasNoPubliclyAccessedMethods, classDeclaration.GetLocation());
            }

            foreach (var method in publicMethodDefinitions)
            {
                var methodSymbol = model.GetDeclaredSymbol(method);
                if (methodSymbol.IsGenericMethod)
                {
                    context.Diagnostics.ReportDiagnostic(WasmDiagnostic.GenericMethodsNotSupported, method.GetLocation());
                    continue; // Skip method
                }
                else
                {
                    if (IsArrayLikeSerializedType(methodSymbol.ReturnType, out var suggestedArrayType))
                    {
                        context.Diagnostics.ReportDiagnostic(WasmDiagnostic.ArrayLikeReturnSerialized, method.ReturnType.GetLocation());
                    }

                    var parametersResult = GetParameterModels(context, model, method.ParameterList).ToList();
                    if (parametersResult == null)
                    {
                        continue; // Skip method
                    }

                    bool isTask = false;
                    ITypeSymbol methodReturnType = methodSymbol.ReturnType;
                    if (typeModelResolver.IsTaskNonGenericType(methodReturnType))
                    {
                        methodReturnType = voidType;
                        isTask = true;
                    }
                    else if (typeModelResolver.IsTaskGenericType(methodReturnType, out var methodTaskReturnType))
                    {
                        methodReturnType = methodTaskReturnType;
                        isTask = true;
                        if (typeModelResolver.IsTaskNonGenericType(methodReturnType) || typeModelResolver.IsTaskGenericType(methodReturnType, out var _))
                        {
                            context.Diagnostics.ReportDiagnostic(WasmDiagnostic.UnwrappedTaskReturnTypesNotSupported, method.ReturnType.GetLocation());
                            continue; // Skip method
                        }
                    }

                    var returnTypeResult = typeModelResolver.GetTypeModel(context, method.ReturnType.GetLocation(), methodReturnType, true);
                    if (returnTypeResult == null)
                    {
                        continue; // Skip method
                    }

                    var wasmMethod = new WasmMethod(
                        name: methodSymbol.Name,
                        parameters: parametersResult.Select(pr => pr.Parameter),
                        returnType: returnTypeResult.WasmType,
                        isAsync: isTask,
                        isStatic: methodSymbol.IsStatic
                    );

                    if (methodNames.Any(m => m.Equals(wasmMethod.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Diagnostics.ReportDiagnostic(WasmDiagnostic.MethodOverloadsNotSupported, method.GetLocation());
                        continue; // Skip method
                    }

                    if (Constants.ReservedMethodNames.Any(m => m.Equals(wasmMethod.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Diagnostics.ReportDiagnostic(WasmDiagnostic.MethodNameReserved, method.GetLocation());
                        continue; // Skip method
                    }

                    methodNames.Add(wasmMethod.Name);

                    yield return new MethodModel(wasmMethod, parametersResult, returnTypeResult, method.ReturnType.GetLocation());
                }
            }
        }

        private IEnumerable<TypeScriptCodeBlock> GetTypeScriptCodeBlocks(IGeneratorContext context, Location location, TypeModel symbolType, List<ITypeSymbol> previouslyVisitedSymbols)
        {
            var currentlyVisitedSymbols = new List<ITypeSymbol>(previouslyVisitedSymbols);
            currentlyVisitedSymbols.Add(symbolType.Symbol);

            if (symbolType.IsTypeKnown == false)
            {
                if (previouslyVisitedSymbols.Any(s => s == symbolType.Symbol))
                {
                    var previouslyVisitedSymbolNames = currentlyVisitedSymbols.Select(c => c.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                    var circularReferenceText = string.Join(" -> ", previouslyVisitedSymbolNames);
                    context.Diagnostics.ReportDiagnostic(WasmDiagnostic.CircularReferenceDetected, location, circularReferenceText);
                    yield break;
                }

                var declaringSyntaxReferences = symbolType.Symbol.DeclaringSyntaxReferences;
                if (declaringSyntaxReferences.Length == 0)
                {
                    context.Diagnostics.ReportDiagnostic(WasmDiagnostic.ParameterNotDefinedInThisAssembly, location);
                    yield break;
                }


                string name = null;
                if (symbolType.Symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
                {
                    name = namedTypeSymbol.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                }
                else
                {
                    name = symbolType.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                }

                var typeScriptProperties = new List<TypeScriptProperty>();
                foreach(var declarationSyntaxReference in declaringSyntaxReferences)
                {
                    var syntax = declarationSyntaxReference.GetSyntax();
                    var model = compilation.GetSemanticModel(declarationSyntaxReference.SyntaxTree);

                    if (syntax is EnumDeclarationSyntax enumDeclarationSyntax)
                    {
                        var members = new List<TypeScriptEnumMember>();
                        foreach (var member in enumDeclarationSyntax.Members)
                        {
                            var memberSymbol = model.GetDeclaredSymbol(member);
                            var value = memberSymbol.ConstantValue as int?;

                            var tsMember = new TypeScriptEnumMember(memberSymbol.Name, value);
                            members.Add(tsMember);
                        }

                        var typeScriptEnum = new TypeScriptEnum(
                            @namespace: symbolType.Symbol.ContainingNamespace?.Name,
                            name: name,
                            members: members
                        );

                        yield return typeScriptEnum;
                    }
                    else
                    {
                        var properties = syntax
                            .ChildNodes()
                            .OfType<PropertyDeclarationSyntax>()
                        .Where(d => d.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)));

                        foreach (var property in properties)
                        {
                            var propertySymbol = model.GetDeclaredSymbol(property);
                            var referenceSymbolType = typeModelResolver.GetTypeModel(context, property.GetLocation(), propertySymbol.Type, typedArray: false);
                            typeScriptProperties.Add(new TypeScriptProperty(propertySymbol.Name, referenceSymbolType.WasmType));

                            var typeScriptCodeBlocks = GetTypeScriptCodeBlocks(context, location, referenceSymbolType, currentlyVisitedSymbols);
                            foreach (var dependencyTypeScriptInterface in typeScriptCodeBlocks)
                            {
                                yield return dependencyTypeScriptInterface;
                            }
                        }

                        var typeScriptInterface = new TypeScriptInterface(
                            @namespace: symbolType.Symbol.ContainingNamespace?.Name,
                            name: name,
                            properties: typeScriptProperties
                        );

                        yield return typeScriptInterface;
                    }
                }
            }

            foreach (var reference in symbolType.ReferencedTypes)
            {
                var codeBlocks = GetTypeScriptCodeBlocks(context, location, reference, currentlyVisitedSymbols);
                foreach (var codeBlock in codeBlocks)
                {
                    yield return codeBlock;
                }
            }
        }

        private IEnumerable<TypeScriptCodeBlock> GetTypeScriptCodeBlocks(IGeneratorContext context, Location location, TypeModel symbolType)
        {
            return GetTypeScriptCodeBlocks(context, location, symbolType, new List<ITypeSymbol> { });
        }

        private List<string> GetUsings(CompilationUnitSyntax compilationUnit)
        {
            List<string> mandatoryUsings = new List<string>
            {
                // IntPtr
                "System",

                // Task
                "System.Threading.Tasks",

                // e.g IntArray32
                "System.Runtime.InteropServices.JavaScript",

                // GCHandle
                "System.Runtime.InteropServices",

                // JsonSerializationHelper
                "Wasm.Sdk",
            };

            var usings = compilationUnit.GetUsings()
                .Union(mandatoryUsings)
                .Distinct()
                .ToList();

            return usings;
        }

        private List<WasmImport> GetImports(string importsFileName, IEnumerable<TypeScriptCodeBlock> codeBlocks)
        {
            var importTypes = codeBlocks
                .Select(st => st.Name.Split('<')[0]) // Type arguments are not present in imports
                .ToList();

            List<WasmImport> imports = new List<WasmImport>();
            if (importTypes.Any())
            {
                imports.Add(new WasmImport(importTypes, importsFileName));
            }

            return imports;
        }

        private List<TypeScriptCodeBlock> GetCodeBlocks(IGeneratorContext context, List<ConstructorModel> constructors, List<MethodModel> methods)
        {
            List<TypeScriptCodeBlock> typeScriptCodeBlocks = new List<TypeScriptCodeBlock>();
            foreach (var constructor in constructors)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    var codeBlocks = GetTypeScriptCodeBlocks(context, parameter.ParameterSyntax.GetLocation(), parameter.SymbolType).ToList();
                    typeScriptCodeBlocks.AddRange(codeBlocks);
                }
            }

            foreach (var method in methods)
            {
                foreach (var parameter in method.Parameters)
                {
                    var codeBlocks = GetTypeScriptCodeBlocks(context, parameter.ParameterSyntax.GetLocation(), parameter.SymbolType).ToList();
                    typeScriptCodeBlocks.AddRange(codeBlocks);
                }

                var returnCodeBlocks = GetTypeScriptCodeBlocks(context, method.ReturnTypeLocation, method.ReturnType).ToList();
                typeScriptCodeBlocks.AddRange(returnCodeBlocks);
            }

            return typeScriptCodeBlocks.Distinct().ToList();
        }

        public ProxyModel GetProxySemanticModel(IGeneratorContext context, ClassDeclarationSyntax classDeclaration, string importsFileName)
        {
            var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var compilationUnit = classDeclaration.GetCompilationUnit();
            var classSymbol = model.GetDeclaredSymbol(classDeclaration);

            var assemblyName = classSymbol.ContainingNamespace.ContainingAssembly.Name;
            var @namespace = classSymbol.ContainingNamespace?.ToString();
            var name = classDeclaration.Identifier.Text;

            if (classSymbol.IsGenericType)
            {
                context.Diagnostics.ReportDiagnostic(WasmDiagnostic.GenericClassesNotSupported, classDeclaration.GetLocation());
            }

            var constructors = GetConstructorModels(model, classDeclaration, context).ToList();
            var methods = GetMethodModels(model, classDeclaration, context).ToList();

            WasmClass classModel = new WasmClass(
                assemblyName,
                @namespace,
                name,
                constructors.Select(cm => cm.Constructor),
                methods.Select(m => m.Method)
            );

            var usings = GetUsings(compilationUnit);
            var codeBlocks = GetCodeBlocks(context, constructors, methods);
            var imports = GetImports(importsFileName, codeBlocks);

            var proxy = new ProxyModel
            {
                Class = classModel,
                Usings = usings,
                CodeBlocks = codeBlocks,
                Imports = imports,
            };

            return proxy;
        }
    }
}
