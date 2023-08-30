using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Wasm.Sdk.Analyzer.Generator;
using Wasm.Sdk.Analyzer.Proxy;
using Wasm.Sdk.Analyzer.Resolvers;

namespace Wasm.Sdk.Analyzer
{
    [Generator]
    public class WasmSourceGenerator : ISourceGenerator
    {
        private Template csharpProxyTemplate;
        private Template typeScriptProxyTemplate;
        private Template typeScriptProxyWorkerTemplate;
        private Template typeScriptProxyWorkerProxyTemplate;
        private Template typeScriptInterfaceTemplate;
        private Template typeScriptIndexTemplate;

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new WasmSyntaxReceiver());

            csharpProxyTemplate = InitializeTemplate(Proxy.Properties.Resources.CSharpProxyTemplate);
            typeScriptProxyTemplate = InitializeTemplate(Proxy.Properties.Resources.TypeScriptProxyTemplate);
            typeScriptProxyWorkerTemplate = InitializeTemplate(Proxy.Properties.Resources.TypeScriptProxyWorkerTemplate);
            typeScriptProxyWorkerProxyTemplate = InitializeTemplate(Proxy.Properties.Resources.TypeScriptProxyWorkerProxyTemplate);
            typeScriptInterfaceTemplate = InitializeTemplate(Proxy.Properties.Resources.TypeScriptInterfaceTemplate);
            typeScriptIndexTemplate = InitializeTemplate(Proxy.Properties.Resources.TypeScriptIndexTemplate);
        }

        private Template InitializeTemplate(string templateText)
        {
            var template = Template.Parse(templateText);
            if (template.HasErrors)
            {
                var errors = string.Join(" | ", template.Messages.Select(x => x.Message));
                throw new InvalidOperationException($"Template parse error: {template.Messages}");
            }

            return template;
        }

        private void SaveTypeScriptCodeBlocks(string jsRootFolder, IEnumerable<TypeScriptCodeBlock> codeBlocks)
        {
            var interfacesTemplate = typeScriptInterfaceTemplate.Render(
                new 
                { 
                    Interfaces = codeBlocks.OfType<TypeScriptInterface>().ToList(),
                    Enums = codeBlocks.OfType<TypeScriptEnum>().ToList(),
                }, 
                member => member.Name
            );

            var interfacesPath = Path.Combine(jsRootFolder, $"{Constants.ProxyWasmTypesFileName}.ts");
            File.WriteAllText(interfacesPath, interfacesTemplate, Encoding.UTF8);
        }

        private void SaveIndexTypescriptFile(string jsRootFolder, List<string> typescriptExports, IEnumerable<TypeScriptCodeBlock> interfaces)
        {
            string[] namedExports = interfaces.Any() ? new[] { $"{Constants.ProxyWasmTypesFileName}" } : new string[0];
            var indexTemplate = typeScriptIndexTemplate.Render(
                new { 
                    DefaultExports = typescriptExports, 
                    NamedExports = namedExports
                }, 
                member => member.Name
            );

            var indexPath = Path.Combine(jsRootFolder, $"{Constants.IndexTypeScriptFileName}.ts");
            File.WriteAllText(indexPath, indexTemplate, Encoding.UTF8);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            IGeneratorContext generatorContext = GeneratorContext.Create(context);

            try
            {
                if (context.SyntaxReceiver is WasmSyntaxReceiver actorSyntaxReciver && actorSyntaxReciver.WasmClassesDeclarations.Any())
                {
                    var typeModelResolver = new TypeModelResolver(context.Compilation);
                    var proxySemanticModelProvider = new ModelProvider(context.Compilation, typeModelResolver);
                    var jsRoot = CreateJsFolder(generatorContext.Options.ProjectRootPath, generatorContext.Options.IntermediateOutputPath);

                    var codeBlocks = new List<TypeScriptCodeBlock>();
                    var typeScriptExports = new List<string>();

                    foreach (var wasmClassDeclaration in actorSyntaxReciver.WasmClassesDeclarations)
                    {
                        var proxyModel = proxySemanticModelProvider.GetProxySemanticModel(generatorContext, wasmClassDeclaration, Constants.ProxyWasmTypesFileName);
                        codeBlocks.AddRange(proxyModel.CodeBlocks);

                        var csharpCode = csharpProxyTemplate.Render(proxyModel, member => member.Name);
                        var typescriptProxyCode = typeScriptProxyTemplate.Render(proxyModel, member => member.Name);
                        var typescriptProxyWorkerCode = typeScriptProxyWorkerTemplate.Render(proxyModel, member => member.Name);
                        var typescriptProxyWorkerProxyCode = typeScriptProxyWorkerProxyTemplate.Render(proxyModel, member => member.Name);

                        File.WriteAllText($"{Path.Combine(jsRoot, proxyModel.Class.Name)}.ts", typescriptProxyCode);
                        File.WriteAllText($"{Path.Combine(jsRoot, proxyModel.Class.Name)}Worker.ts", typescriptProxyWorkerCode);
                        File.WriteAllText($"{Path.Combine(jsRoot, proxyModel.Class.Name)}WorkerProxy.ts", typescriptProxyWorkerProxyCode);
                        typeScriptExports.Add(proxyModel.Class.Name);
                        typeScriptExports.Add($"{proxyModel.Class.Name}Worker");
                        typeScriptExports.Add($"{proxyModel.Class.Name}WorkerProxy");

                        context.AddSource($"{proxyModel.Class.Name}.g", SourceText.From(csharpCode, Encoding.UTF8));
                    }

                    var distinctCodeBlocks = codeBlocks.Distinct();
                    SaveTypeScriptCodeBlocks(jsRoot, distinctCodeBlocks);
                    SaveIndexTypescriptFile(jsRoot, typeScriptExports, distinctCodeBlocks);

                    AddSharedTypescriptFile(jsRoot);
                }
            }
            catch (Exception e)
            {
                //generatorContext.Diagnostics.ReportDiagnostic(WasmDiagnostic.Exception, Location.None, e.ToString());
            }
        }

        private string CreateJsFolder(string projectRootPath, string relativeIntermediateOutputPath)
        {
            // Shared ts file
            var jsRootFolder = Path.Combine(projectRootPath, relativeIntermediateOutputPath, "ts");
            if (Directory.Exists(jsRootFolder))
            {
                Directory.Delete(jsRootFolder, true);
            }
            Directory.CreateDirectory(jsRootFolder);

            return jsRootFolder;
        }

        private void AddSharedTypescriptFile(string jsRootFolder)
        {
            var wasmSharedPath = Path.Combine(jsRootFolder, "wasmShared.ts");
            File.WriteAllText(wasmSharedPath, Proxy.Properties.Resources.wasmShared, Encoding.UTF8);
        }
    }
}
