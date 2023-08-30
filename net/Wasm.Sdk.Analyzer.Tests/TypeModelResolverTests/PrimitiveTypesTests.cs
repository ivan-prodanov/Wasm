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
    public class PrimitiveTypesTests : TypeTests
    {
        [Fact]
        public void Test_bool_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public bool i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "bool");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "bool");
            Assert.True(typeModel.WasmType.TsType == "boolean");
        }

        [Fact]
        public void Test_byte_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public byte i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "byte");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "byte");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_sbyte_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public sbyte i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "sbyte");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "sbyte");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_char_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public char i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "char");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "char");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_decimal_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public decimal s { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "decimal");
            Assert.True(typeModel.WasmType.MonoSignature == "d");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "decimal");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_double_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public double s { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "double");
            Assert.True(typeModel.WasmType.MonoSignature == "d");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "double");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_float_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public float s { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "float");
            Assert.True(typeModel.WasmType.MonoSignature == "f");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "float");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_int_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public int i { get; set; }
}"
            );
            
            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "int");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "int");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_uint_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public uint i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "uint");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "uint");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_long_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public long i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "long");
            Assert.True(typeModel.WasmType.MonoSignature == "l");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "long");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_ulong_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public ulong i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "ulong");
            Assert.True(typeModel.WasmType.MonoSignature == "l");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "ulong");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_short_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public short i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "short");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "short");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_ushort_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public ushort i { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "ushort");
            Assert.True(typeModel.WasmType.MonoSignature == "i");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "ushort");
            Assert.True(typeModel.WasmType.TsType == "number");
        }

        [Fact]
        public void Test_string_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public string s { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Assert.True(typeModel.WasmType.ManagedType == "string");
            Assert.True(typeModel.WasmType.MonoSignature == "s");
            Assert.True(typeModel.WasmType.Namespace == "");
            Assert.True(typeModel.WasmType.ProxyType == "string");
            Assert.True(typeModel.WasmType.TsType == "string");
        }

        [Fact]
        public void Test_object_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public object s { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);
            TypeModelAsserts.AssertObject(typeModel);
        }
    }
}
