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
    public class GenericTypePrimitiveTests : TypeTests
    {
        [Fact]
        public void Test_single_generic_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<U>{}
    public Generic<int> i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.False(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);
            Assert.True(typeModel.ReferencedTypes[0].IsTypeKnown);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<int>");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<number>");
        }

        [Fact]
        public void Test_single_generic_property_array()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<U>{}
    public Generic<int[]> i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.False(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);
            Assert.True(typeModel.ReferencedTypes[0].IsTypeKnown);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<int[]>");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<number[]>");
        }

        [Fact]
        public void Test_single_generic_property_generic_array()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<U>{}
    public Generic<Generic<int[]>> i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.False(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);
            Assert.False(typeModel.ReferencedTypes[0].IsTypeKnown);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<Generic<int[]>>");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<Generic<number[]>>");
        }

        [Fact]
        public void Test_single_generic_property_array_generic_array()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<U>{}
    public Generic<Generic<int[]>[]> i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.False(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);
            Assert.True(typeModel.ReferencedTypes[0].IsTypeKnown);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<Generic<int[]>[]>");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<Generic<number[]>[]>");
        }

        [Fact]
        public void Test_multiple_generic_property_array_generic_array()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<U, V>{}
    public Generic<Generic<int[], string[]>[], Generic<Generic<string[], string>[], string[]>[]> i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.False(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 2);
            Assert.True(typeModel.ReferencedTypes[0].IsTypeKnown);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<Generic<int[], string[]>[], Generic<Generic<string[], string>[], string[]>[]>");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<Generic<number[], string[]>[], Generic<Generic<string[], string>[], string[]>[]>");
        }

        [Fact]
        public void Test_multiple_array_generic_property_array_generic_array()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<U, V>{}
    public Generic<Generic<int[], string[]>[], Generic<Generic<string[], string>[], string[]>[]>[] i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 1);
            Assert.False(typeModel.ReferencedTypes[0].IsTypeKnown);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<Generic<int[], string[]>[], Generic<Generic<string[], string>[], string[]>[]>[]");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<Generic<number[], string[]>[], Generic<Generic<string[], string>[], string[]>[]>[]");
        }

        [Fact]
        public void Test_multiple_generic_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    class Generic<A, B, C, D, E, F, G, H, I, J, K, L>{}
    public Generic<byte, sbyte, float, double, int, uint, long, ulong, short, ushort, string, object> i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.False(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 12);

            Assert.True(typeModel.WasmType.ManagedType == "Generic<byte, sbyte, float, double, int, uint, long, ulong, short, ushort, string, object>");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "Generic<number, number, number, number, number, number, number, number, number, number, string, any>");
        }
    }
}
