using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    class WasmMethodBase
    {
        public WasmMethodBase(IEnumerable<WasmParameter> parameters)
        {
            parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Parameters = new List<WasmParameter>(parameters);
        }

        public List<WasmParameter> Parameters { get; }
    }
}
