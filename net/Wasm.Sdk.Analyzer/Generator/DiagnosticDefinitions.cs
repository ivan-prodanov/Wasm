using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Generator
{
    public enum WasmDiagnostic
    {
        Exception,
        ArrayLikeParameterSerialized,
        ArrayLikeReturnSerialized,
        ClassHasNoPublicConstructor,
        ClassHasNoMethods,
        ClassHasNoPubliclyAccessedMethods,
        GenericMethodsNotSupported,
        GenericClassesNotSupported,
        UnsupportedParameterType,
        ParameterNotDefinedInThisAssembly,
        MultiDimensionalArraysNotSupported,
        UnwrappedTaskReturnTypesNotSupported,
        MethodOverloadsNotSupported,
        MethodNameReserved,
        CircularReferenceDetected,
    }

    static class DiagnosticProvider
    {
        private static Dictionary<WasmDiagnostic, (DiagnosticSeverity severity, string messageFormat)> wasmDiagnosticMap = new Dictionary<WasmDiagnostic, (DiagnosticSeverity, string messageFormat)>
        {
            [WasmDiagnostic.Exception] = (DiagnosticSeverity.Error, "Error in source generator: '{0}'."),
            [WasmDiagnostic.ArrayLikeParameterSerialized] = (DiagnosticSeverity.Error, "Parameter will be serialized. If you wish to optimize this, use {0}[]."),
            [WasmDiagnostic.ArrayLikeReturnSerialized] = (DiagnosticSeverity.Error, "Method return value will be serialized. If you wish to optimize this, use {0}[]."),
            [WasmDiagnostic.ClassHasNoPublicConstructor] = (DiagnosticSeverity.Error, "Class has no public constructor available."),
            [WasmDiagnostic.ClassHasNoMethods] = (DiagnosticSeverity.Error, "Class has no methods."),
            [WasmDiagnostic.ClassHasNoPubliclyAccessedMethods] = (DiagnosticSeverity.Error, "Class has no publicly accessible methods."),
            [WasmDiagnostic.GenericMethodsNotSupported] = (DiagnosticSeverity.Error, "Generic Methods are not supported yet."),
            [WasmDiagnostic.GenericClassesNotSupported] = (DiagnosticSeverity.Error, "Generic Classes are not supported yet."),
            [WasmDiagnostic.UnsupportedParameterType] = (DiagnosticSeverity.Error, "Parameters of type {0} are not supported yet."),
            [WasmDiagnostic.ParameterNotDefinedInThisAssembly] = (DiagnosticSeverity.Error, "The type of the parameter must be declared within the same assembly as the method using it."),
            [WasmDiagnostic.MultiDimensionalArraysNotSupported] = (DiagnosticSeverity.Error, "Multidimensional arrays are not supported."),
            [WasmDiagnostic.UnwrappedTaskReturnTypesNotSupported] = (DiagnosticSeverity.Error, "Methods returning unwrapped tasks (Task of Task) are not supported"),
            [WasmDiagnostic.MethodOverloadsNotSupported] = (DiagnosticSeverity.Error, "Method overloads are not supported"),
            [WasmDiagnostic.MethodNameReserved] = (DiagnosticSeverity.Error, "Method name is reserved. Please use another name."),
            [WasmDiagnostic.CircularReferenceDetected] = (DiagnosticSeverity.Warning, "Circular Reference Detected: {0}"),
        };

        public static DiagnosticDescriptor GetDescriptor(WasmDiagnostic diagnostic, params string[] messageArgs)
        {
            DiagnosticDescriptor descriptor = null;
            if (wasmDiagnosticMap.TryGetValue(diagnostic, out (DiagnosticSeverity severity, string messageFormat) seveurityMessage))
            {
                descriptor = new DiagnosticDescriptor(
                    $"SG{(int)diagnostic:D4}",
                    $"SG{(int)diagnostic:D4}: Error in source generator",
                    string.Format(seveurityMessage.messageFormat, messageArgs),
                    "SourceGenerator",
                    seveurityMessage.severity,
                    true
                );
            }
            else
            {
#if DEBUG
                throw new InvalidOperationException($"Diagnostic {diagnostic} was not defined");
#endif
            }

            return descriptor;
        }
    }
}
