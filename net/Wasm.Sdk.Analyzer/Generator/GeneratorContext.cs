using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Wasm.Sdk.Analyzer.Generator
{
    internal class GeneratorContext : IGeneratorContext
    {
        private GeneratorContext(GeneratorExecutionContext generatorExecutionContext)
        {
            Options = new GeneratorOptions(generatorExecutionContext);
            Diagnostics = new GeneratorDiagnostics(generatorExecutionContext, Options);
            GeneratorExecutionContext = generatorExecutionContext;
        }

        public IGeneratorOptions Options { get; }
        public IGeneratorDiagnostics Diagnostics { get; }
        public GeneratorExecutionContext GeneratorExecutionContext { get; }

        public static GeneratorContext Create(GeneratorExecutionContext context)
        {
            var sourceGenContext = new GeneratorContext(context);

            if (sourceGenContext.Options.EnableDebug)
            {
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
            }
            return sourceGenContext;
        }
    }
}