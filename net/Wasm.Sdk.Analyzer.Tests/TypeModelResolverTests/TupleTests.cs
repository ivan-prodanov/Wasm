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
    public class TupleTests : TypeTests
    {
        [Fact]
        public void Test_tuple()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public (bool b, string s) i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 2);

            Assert.True(typeModel.WasmType.ManagedType == "(bool b, string s)");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "[boolean, string]");
        }

        [Fact]
        public void Test_tuple2()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<T>{}

    public Generic<(bool b, Generic<(string s, Generic<Generic<long>[]>[])[]>[])>[] i { get; set; }
}"
            );



            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<(bool b, Generic<(string s, Generic<Generic<long>[]>[])[]>[])>[]");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<[boolean, Generic<[string, Generic<Generic<number>[]>[]][]>[]]>[]");
        }
    }
    class Class1
    {
        class Generic<T> { }

        Generic<(bool b, Generic<(string s, Generic<Generic<long>[]>[])[]>[])>[] asdasdi { get; set; }
    }
}
