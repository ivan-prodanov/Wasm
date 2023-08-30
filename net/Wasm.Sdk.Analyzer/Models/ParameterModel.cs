using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Wasm.Sdk.Analyzer.Proxy;

namespace Wasm.Sdk.Analyzer.Models
{
    [DebuggerDisplay("Parameter {Parameter.Name}; TS = {Parameter.Type.TsType}; C# = {Parameter.Type.ManagedType}")]
    class ParameterModel
    {
        public ParameterModel(ParameterSyntax parameterSyntax, TypeModel typeModel, WasmParameter parameter)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            SymbolType = typeModel ?? throw new ArgumentNullException(nameof(typeModel));
            ParameterSyntax = parameterSyntax ?? throw new ArgumentNullException(nameof(parameterSyntax));
        }

        public WasmParameter Parameter { get; }
        public TypeModel SymbolType { get; }
        public ParameterSyntax ParameterSyntax { get; }
    }
}
