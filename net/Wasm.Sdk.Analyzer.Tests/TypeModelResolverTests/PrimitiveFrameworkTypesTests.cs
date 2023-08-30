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
    public class PrimitiveFrameworkTypesTests : TypeTests
    {
        [Fact]
        public void Test_bool_property()
        {
            var (resolver, context, property) = ArrangeProperty(@"
class Class1
{
    public System.Boolean i { get; set; }
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
    public System.Byte i { get; set; }
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
    public System.SByte i { get; set; }
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
    public System.Char i { get; set; }
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
    public System.Decimal s { get; set; }
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
    public System.Double s { get; set; }
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
    public System.Single s { get; set; }
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
    public System.Int32 i { get; set; }
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
    public System.UInt32 i { get; set; }
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
    public System.Int64 i { get; set; }
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
    public System.UInt64 i { get; set; }
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
    public System.Int16 i { get; set; }
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
    public System.UInt16 i { get; set; }
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
    public System.String s { get; set; }
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
    public System.Object s { get; set; }
}"
            );

            var typeModel = resolver.GetTypeModel(context, Location.None, property.Type, false);

            Assert.True(typeModel.IsTypeKnown);
            Assert.True(typeModel.ReferencedTypes.Count == 0);

            Asserts.TypeModelAsserts.AssertObject(typeModel);
        }
    }
}
