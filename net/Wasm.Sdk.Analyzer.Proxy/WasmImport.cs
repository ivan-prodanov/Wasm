using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    class WasmImport
    {
        public WasmImport(List<string> types, string file)
        {
            Types = types;
            File = file;
        }

        public List<string> Types { get; }
        public string File { get; }
    }
}
