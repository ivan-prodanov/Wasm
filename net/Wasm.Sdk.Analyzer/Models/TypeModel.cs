using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Wasm.Sdk.Analyzer.Proxy;

namespace Wasm.Sdk.Analyzer.Models
{

    [DebuggerDisplay("IsKnown = {IsTypeKnown}; Name = {Symbol.Name}; Children = {ReferencedTypes.Count}")]
    class TypeModel
    {
        public TypeModel(WasmType wasmType, ITypeSymbol symbol, IEnumerable<TypeModel> referencedTypes, bool isTypeKnown)
        {
            WasmType = wasmType ?? throw new ArgumentNullException(nameof(wasmType));
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            IsTypeKnown = isTypeKnown;
            referencedTypes = referencedTypes ?? throw new ArgumentNullException(nameof(referencedTypes));
            ReferencedTypes = new List<TypeModel>(referencedTypes);
        }

        // Contains no references
        public TypeModel(WasmType wasmType, ITypeSymbol symbol, bool isTypeKnown) : this(wasmType, symbol, Enumerable.Empty<TypeModel>(), isTypeKnown)
        {
        }

        public WasmType WasmType { get; }
        public ITypeSymbol Symbol { get; }
        public bool IsTypeKnown { get; }
        public IReadOnlyList<TypeModel> ReferencedTypes { get; }
    }
}
