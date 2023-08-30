using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Wasm.Sdk.Analyzer.Proxy;

namespace Wasm.Sdk.Analyzer.Models
{

    [DebuggerDisplay("Ctor Parameters = {Parameters.Length}")]
    class ConstructorModel
    {
        public ConstructorModel(WasmConstructor wasmConstructor, IEnumerable<ParameterModel> parameters)
        {
            Constructor = wasmConstructor ?? throw new ArgumentNullException(nameof(wasmConstructor));
            parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Parameters = new List<ParameterModel>(parameters);
        }

        public WasmConstructor Constructor { get; set; }
        public IReadOnlyList<ParameterModel> Parameters { get; set; }
    }
}
