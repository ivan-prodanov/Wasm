using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    class WasmConstructor : WasmMethodBase
    {
        public WasmConstructor(IEnumerable<WasmParameter> parameters)
            : base(parameters)
        {
        }

        public string ProxyName { get; internal set; }

        public string TsUnionParams { get; }
    }
}
