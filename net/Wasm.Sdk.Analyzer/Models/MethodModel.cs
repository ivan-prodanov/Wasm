using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Wasm.Sdk.Analyzer.Proxy;

namespace Wasm.Sdk.Analyzer.Models
{

    [DebuggerDisplay("Method = {Method.Name}; Async = {Method.IsAsync}; ReturnType = {ReturnType.IsKnown}")]
    class MethodModel
    {
        public MethodModel(WasmMethod wasmMethod, IEnumerable<ParameterModel> parameters, TypeModel returnType, Location returnTypeLocation)
        {
            Method = wasmMethod ?? throw new ArgumentNullException(nameof(wasmMethod));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ReturnTypeLocation = returnTypeLocation ?? throw new ArgumentNullException(nameof(returnTypeLocation));
            Parameters = new List<ParameterModel>(parameters);
        }

        public WasmMethod Method { get; set; }
        public TypeModel ReturnType { get; }
        public Location ReturnTypeLocation { get; }
        public IReadOnlyList<ParameterModel> Parameters { get; set; }
    }
}
