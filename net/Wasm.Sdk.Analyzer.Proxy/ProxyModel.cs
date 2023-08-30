using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    class ProxyModel
    {
        public WasmClass Class { get; set; }
        public List<string> Usings { get; set; } = new List<string>();
        public List<WasmImport> Imports { get; set; } = new List<WasmImport>();
        public List<TypeScriptCodeBlock> CodeBlocks { get; set; } = new List<TypeScriptCodeBlock>();
    }
}
