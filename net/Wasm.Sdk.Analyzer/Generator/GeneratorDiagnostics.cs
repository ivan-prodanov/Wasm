using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wasm.Sdk.Analyzer.Generator
{
    internal class GeneratorDiagnostics : IGeneratorDiagnostics
    {
        private readonly GeneratorExecutionContext _generatorExecutionContext;

        public GeneratorDiagnostics(GeneratorExecutionContext generatorExecutionContext, IGeneratorOptions options)
        {
            _generatorExecutionContext = generatorExecutionContext;
        }

        public void ReportDiagnostic(WasmDiagnostic diagnosticType, Location location, params string[] messageArgs)
        {
            location = location ?? Location.None;

            var descriptor = DiagnosticProvider.GetDescriptor(diagnosticType, messageArgs);
            if (descriptor != null)
            {
                _generatorExecutionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        descriptor,
                        location,
                        "Wasm.Sdk.Analyzer"
                    )
                );

                if (descriptor.DefaultSeverity == DiagnosticSeverity.Error)
                {
                    var errorMessage = string.Format(descriptor.MessageFormat.ToString(), messageArgs);
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }
    }
}