using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasm.Sdk.Analyzer.Generator;
using Wasm.Sdk.Analyzer.Resolvers;
using Xunit;

namespace Wasm.Sdk.Analyzer.Tests.TypeModelResolverTests
{
    public class GenericArgumentTests : TypeTests
    {
        [Fact]
        public void Test_single_generic_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1<T>
{
    public T i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "T");
            Assert.True(typeModel.WasmType.MonoSignature == "o");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "T");
        }
    }
}
