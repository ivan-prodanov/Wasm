using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wasm.Sdk.Analyzer
{
    static class Constants
    {
        static Constants()
        {
            SupportedInteropTypes = ArrayInteropMap.Keys.ToList();
        }

        public const string ProxyWasmTypesFileName = "wasmTypes";
        public const string IndexTypeScriptFileName = "index";

        public static IReadOnlyList<TypeKind> SerialiazableTypeKinds { get; } = new List<TypeKind>
        {
            TypeKind.Enum,
            TypeKind.Interface,
            TypeKind.Class,
            TypeKind.Struct,
        };


        public static IReadOnlyList<SpecialType> PrimitiveSpecialTypes { get; } = new List<SpecialType>
        {
            SpecialType.System_Void,

            SpecialType.System_Boolean,

            SpecialType.System_Byte,
            SpecialType.System_SByte,

            SpecialType.System_Int16,
            SpecialType.System_UInt16,

            SpecialType.System_Int32,
            SpecialType.System_UInt32,

            SpecialType.System_Int64,
            SpecialType.System_UInt64,

            SpecialType.System_Single,
            SpecialType.System_Double,
            SpecialType.System_Decimal,

            SpecialType.System_Char,
            SpecialType.System_String,
            SpecialType.System_Object,
            SpecialType.System_IntPtr,
            SpecialType.System_UIntPtr,
        };



        // TODO: Consider DateTime, DateTimeOffset, TimeSpan and Guid
        public static IReadOnlyList<string> PrimitiveTypes { get; } = new List<string>
        {
            "void",
            "bool",
            "byte",
            "sbyte",
            "char",
            "decimal",
            "double",
            "float",
            "int",
            "uint",
            "long",
            "ulong",
            "short",
            "ushort",
            "string",
        };

        public static IReadOnlyDictionary<string, string> ArrayInteropMap { get; } = new Dictionary<string, string>
        {
            ["byte"] = "Uint8Array",
            ["sbyte"] = "Int8Array",
            ["short"] = "Int16Array",
            ["ushort"] = "UInt16Array",
            ["int"] = "Int32Array",
            ["uint"] = "UInt32Array",
            ["float"] = "Float32Array",
            ["double"] = "Float64Array",
        };



        public static IReadOnlyList<string> ExcludedSerializationTypes = new List<string>
        {
            "System.Threading.Tasks.Task",
            "System.Threading.Tasks.ValueTask",
            "System.Action",
            "System.Delegate",
            "System.MulticastDelegate",
            "System.IO.Stream"
        };

        public static IReadOnlyList<string> ReservedMethodNames = new List<string>
        {
            "Free",
        };

        public static IReadOnlyList<string> SupportedInteropTypes { get; }
    }
}
