using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasm.Sdk.Analyzer.Generator;
using Wasm.Sdk.Analyzer.Resolvers;
using Wasm.Sdk.Analyzer.Tests.Asserts;
using Xunit;

namespace Wasm.Sdk.Analyzer.Tests.TypeModelResolverTests
{
    public class JaggedArrayTests : TypeTests
    {
        [Fact]
        public void Test_jagged_Array()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public bool[][] i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);

            Assert.True(typeModel.WasmType.ManagedType == "bool[][]");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "boolean[][]");
        }

        [Fact]
        public void Test_jagged_Array_3()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public bool[][][] i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);

            Assert.True(typeModel.WasmType.ManagedType == "bool[][][]");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "boolean[][][]");
        }

        [Fact]
        public void Test_jagged_Array_4()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public bool[][][][] i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);

            Assert.True(typeModel.WasmType.ManagedType == "bool[][][][]");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "boolean[][][][]");
        }
    }
}
