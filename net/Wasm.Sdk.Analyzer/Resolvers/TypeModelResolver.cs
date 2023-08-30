using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wasm.Sdk.Analyzer.Extensions;
using Wasm.Sdk.Analyzer.Generator;
using Wasm.Sdk.Analyzer.Models;
using Wasm.Sdk.Analyzer.Proxy;

namespace Wasm.Sdk.Analyzer.Resolvers
{
    class TypeModelResolver
    {
        private readonly INamedTypeSymbol ienumerableType;
        private readonly INamedTypeSymbol ienumerableGenericType;
        private readonly INamedTypeSymbol taskType;
        private readonly INamedTypeSymbol valueTaskType;
        private readonly INamedTypeSymbol taskGenericType;
        private readonly INamedTypeSymbol valueTaskGenericType;
        private readonly INamedTypeSymbol iDictionaryType;
        private readonly INamedTypeSymbol iReadOnlyDictionaryType;
        private readonly IReadOnlyList<INamedTypeSymbol> unsupportedTypes;
        private readonly IReadOnlyList<INamedTypeSymbol> taskReturnTypes;

        public TypeModelResolver(Compilation compilation)
        {

            ienumerableType = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
            ienumerableGenericType = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");

            taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            taskGenericType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
            valueTaskGenericType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

            // TODO see if these two can be replaced using IEnumerable`KeyValuePair`2`. Essentially they can, but we have to find out how to find the symbol for  IEnumerable`KeyValuePair`2
            iDictionaryType = compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");
            iReadOnlyDictionaryType = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");
            
            unsupportedTypes = new List<INamedTypeSymbol>
            {
                valueTaskType,
                valueTaskGenericType,
                ienumerableType,
                compilation.GetTypeByMetadataName("System.Span`1"),
                compilation.GetTypeByMetadataName("System.ReadOnlySpan`1"),
                compilation.GetTypeByMetadataName("System.Memory`1"),
                compilation.GetTypeByMetadataName("System.Delegate"),
            };
            
            taskReturnTypes = new List<INamedTypeSymbol>
            {
                taskType,
                taskGenericType,
            };
        }

        public bool IsTaskType(ITypeSymbol typeSymbol)
        {
            return taskReturnTypes.Contains(typeSymbol);
        }

        public bool IsTaskNonGenericType(ITypeSymbol typeSymbol)
        {
            return typeSymbol == taskType;
        }

        public bool IsTaskGenericType(ITypeSymbol typeSymbol, out ITypeSymbol typeParameter)
        {
            typeParameter = null;
            var isGenericTask = false;

            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsDerivedFromClassOrInterface(taskGenericType))
            {
                isGenericTask = true;
                typeParameter = namedTypeSymbol.TypeArguments.First();
            }

            return isGenericTask;
        }

        public TypeModel GetTypeModel(IGeneratorContext context, Location syntaxLocation, ITypeSymbol typeSymbol, bool typedArray)
        {
            bool nullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
            if (nullable)
            {
                if (typeSymbol is INamedTypeSymbol nullableNamedSymbol && nullableNamedSymbol.TypeArguments.Any())
                {
                    typeSymbol = nullableNamedSymbol.TypeArguments.First();
                }
                else
                {
                    typeSymbol = typeSymbol.OriginalDefinition;
                }
            }
            WasmType wasmType;
            var referencedTypes = new List<TypeModel>();
            var typeArguments = new List<WasmType>();
            bool isTypeKnown = true;
            var symbolName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (unsupportedTypes.Contains(typeSymbol))
            {
                context.Diagnostics.ReportDiagnostic(WasmDiagnostic.UnsupportedParameterType, syntaxLocation, typeSymbol.Name);
                return null;
            }

            if (typeSymbol.TypeKind == TypeKind.Delegate)
            {
                context.Diagnostics.ReportDiagnostic(WasmDiagnostic.UnsupportedParameterType, syntaxLocation, typeSymbol.Name);
                return null;
            }

            if (typeSymbol.SpecialType == SpecialType.System_Collections_IEnumerator)
            {
                context.Diagnostics.ReportDiagnostic(WasmDiagnostic.UnsupportedParameterType, syntaxLocation, typeSymbol.Name);
                return null;
            }
            if (typeSymbol.TypeKind == TypeKind.Enum)
            {

            }

            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
            if (namedTypeSymbol != null)
            {
                foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                {
                    var wasmTypeArgumentResult = GetTypeModel(context, syntaxLocation, typeArgument, false);
                    if (wasmTypeArgumentResult == null)
                    {
                        return null;
                    }
                    referencedTypes.Add(wasmTypeArgumentResult);

                    typeArguments.Add(wasmTypeArgumentResult.WasmType);
                }
            }

            // Primitive
            if (typeSymbol.TypeKind == TypeKind.TypeParameter)
            {
                wasmType = WasmType.CreateGenericTypeArgument(symbolName, nullable);
            }
            else if (typeSymbol.IsPrimitive())
            {
                wasmType = WasmType.CreatePrimitive(symbolName, nullable);
                // Primitive
            }
            // Array
            else if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                if (arrayTypeSymbol.Rank > 1)
                {
                    context.Diagnostics.ReportDiagnostic(WasmDiagnostic.MultiDimensionalArraysNotSupported, syntaxLocation);
                    return null;
                }

                if (typedArray && Constants.ArrayInteropMap.TryGetValue(arrayTypeSymbol.ElementType.ToString(), out var interopType))
                {
                    wasmType = WasmType.CreateTypedArray(interopType, interopType, arrayTypeSymbol.ElementType.ToString(), nullable);
                }
                else
                {
                    var arrayElementModel = GetTypeModel(context, syntaxLocation, arrayTypeSymbol.ElementType, false);
                    referencedTypes.Add(arrayElementModel);
                    wasmType = WasmType.CreateArray(symbolName, arrayElementModel.WasmType, nullable);
                }
            }
            else if (typeSymbol.IsTupleType)
            {
                wasmType = WasmType.CreateTuple(symbolName, typeArguments, nullable);
            }
            else if (namedTypeSymbol?.IsDerivedFromClassOrInterface(iDictionaryType) == true
                || namedTypeSymbol?.IsDerivedFromClassOrInterface(iReadOnlyDictionaryType) == true)
            {
                wasmType = WasmType.CreateDictionary(symbolName, typeArguments[0], typeArguments[1], nullable);
            }
            else if (namedTypeSymbol?.IsDerivedFromClassOrInterface(ienumerableGenericType) == true)
            {
                wasmType = WasmType.CreateArray(symbolName, typeArguments.First(), nullable);
            }
            else
            {
                isTypeKnown = false;
                wasmType = WasmType.CreateObject(typeSymbol.ContainingNamespace?.ToString(), symbolName, typeSymbol.Name, nullable, typeArguments.ToArray());
            }

            return new TypeModel(wasmType, typeSymbol, referencedTypes, isTypeKnown);
        }
    }
}
