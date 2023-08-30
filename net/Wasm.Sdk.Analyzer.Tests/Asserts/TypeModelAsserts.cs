using System;
using System.Collections.Generic;
using System.Text;
using Wasm.Sdk.Analyzer.Models;
using Xunit;

namespace Wasm.Sdk.Analyzer.Tests.Asserts
{
    static class TypeModelAsserts
    {
        public static void AssertObject(TypeModel typeModel)
        {
            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "object");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "any");
        }
    }
}
