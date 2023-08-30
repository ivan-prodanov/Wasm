using Microsoft.CodeAnalysis;

namespace Wasm.Sdk.Analyzer.Generator
{
    public interface IGeneratorContext
    {
        IGeneratorDiagnostics Diagnostics { get; }
        IGeneratorOptions Options { get; }
    }
}