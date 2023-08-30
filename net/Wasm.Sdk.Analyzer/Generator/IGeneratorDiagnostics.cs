using Microsoft.CodeAnalysis;

namespace Wasm.Sdk.Analyzer.Generator
{
    public interface IGeneratorDiagnostics
    {
        void ReportDiagnostic(WasmDiagnostic diagnosticType, Location location, params string[] messageArgs);
    }
}