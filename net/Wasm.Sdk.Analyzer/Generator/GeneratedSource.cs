using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Generator
{
    internal class GeneratedSource
    {
        public GeneratedSource(string sourceCode, string fileName)
        {
            SourceCode = sourceCode;
            FileName = fileName;
        }

        public string SourceCode { get; set; }
        public string FileName { get; set; }
    }
}
