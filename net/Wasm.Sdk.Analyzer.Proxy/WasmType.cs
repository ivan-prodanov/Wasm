using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasm.Sdk.Analyzer.Proxy
{
    public enum TypeConversion
    {
        None,
        TypedArray,
        Serialize,
    }
    

    public class WasmType
    {
        private WasmType(string @namespace, string managedType, string proxyType, string tsType, string monoSignature, string typedArrayElementType, bool nullable, TypeConversion typeConversion)
        {
            Namespace = @namespace ?? string.Empty;
            if (Namespace == "<global namespace>")
            {
                Namespace = "";
            }
            ManagedType = managedType;
            ProxyType = proxyType;
            TsType = tsType;
            MonoSignature = monoSignature;
            TypedArrayElementType = typedArrayElementType;
            TypeConversion = typeConversion;
            Nullable = nullable;
        }

        public string Namespace { get; }
        public string ManagedType { get; }
        public string ProxyType { get; }

        public string TsType { get; }
        public bool Nullable { get; }

        public string MonoSignature { get; }
        public string TypedArrayElementType { get; }
        public TypeConversion TypeConversion { get; }

        private static string ManagedTypeToTsType(string managedType)
        {
            var tsType = managedType.ToLower() switch
            {
                "bool" => "boolean",
                "byte" => "number",
                "sbyte" => "number",
                "char" => "number",
                "decimal" => "number",
                "double" => "number",
                "float" => "number",
                "int" => "number",
                "uint" => "number",
                "long" => "number",
                "ulong" => "number",
                "short" => "number",
                "ushort" => "number",
                "string" => "string",
                "object" => "any",
                "intptr" => "number",
                "uintptr" => "number",
                "void" => "void",
                _ => managedType,
            };

            return tsType;
        }

        private static string ManagedPrimitiveToMonoSignature(string managedType)
        {
            var monoSignature = managedType.ToLower() switch
            {
                "bool" => "i",
                "byte" => "i",
                "sbyte" => "i",
                "char" => "i",
                "decimal" => "d", // TODO, Add support
                "double" => "d",
                "float" => "f",
                "int" => "i",
                "uint" => "i",
                "long" => "l",
                "ulong" => "l",
                "short" => "i",
                "ushort" => "i",
                "string" => "s",
                "object" => "s",
                "intptr" => "i",
                "uintptr" => "i",
                "void" => "",
                _ => "",
            };

            return monoSignature;
        }

        public static WasmType CreatePrimitive(string managedType, bool nullable)
        {
            managedType = managedType ?? throw new ArgumentNullException(nameof(managedType));

            var tsType = ManagedTypeToTsType(managedType);
            var monoSignature = ManagedPrimitiveToMonoSignature(managedType);

            var proxyType = managedType;
            if (managedType == "object")
            {
                proxyType = "string";
            }

            return new WasmType(string.Empty, managedType, proxyType, tsType, monoSignature, string.Empty, nullable, TypeConversion.None);
        }

        public static WasmType CreateArray(string managedType, WasmType wasmType, bool nullable)
        {
            return new WasmType(wasmType.Namespace, managedType, "string", $"{wasmType.TsType}[]", "s", string.Empty, nullable, TypeConversion.Serialize);
        }

        public static WasmType CreateDictionary(string managedType, WasmType key, WasmType value, bool nullable)
        {
            var tsType = $"{{ [key: {key.TsType}]: {value.TsType}; }}";

            return new WasmType(string.Empty, managedType, "string", tsType, "s", string.Empty, nullable, TypeConversion.Serialize);
        }

        public static WasmType CreateTuple(string managedType, IEnumerable<WasmType> tupleChildren, bool nullable)
        {
            var tsTypes = tupleChildren.Select(tp => tp.TsType);
            var tsType = $"[{string.Join(", ", tsTypes)}]";

            return new WasmType(string.Empty, managedType, "string", tsType, "s", string.Empty, nullable, TypeConversion.Serialize);
        }

        public static WasmType Void = WasmType.CreatePrimitive("void", false);

        public static WasmType CreateTypedArray(string managedType, string tsType, string typedArrayElementType, bool nullable)
        {
            return new WasmType(string.Empty, managedType, tsType, tsType, "o", typedArrayElementType, nullable, TypeConversion.TypedArray);
        }

        public static WasmType CreateGenericTypeArgument(string name, bool nullable)
        {
            return new WasmType(string.Empty, name, "string", name, "o", string.Empty, nullable, TypeConversion.Serialize);
        }

        public static WasmType CreateObject(string @namespace, string managedTypeFull, string managedType, bool nullable, params WasmType[] typeArguments)
        {
            if (typeArguments.Any())
            {
                var typeArgTypes = typeArguments.Select(ta => ta.TsType);
                var typeArgsString = string.Join(", ", typeArgTypes);
                return new WasmType(@namespace, managedTypeFull, "string", $"{managedType}<{typeArgsString}>", "s", string.Empty, nullable, TypeConversion.Serialize);
            }
            else
            {
                return new WasmType(@namespace, managedTypeFull, "string", managedType, "s", string.Empty, nullable, TypeConversion.Serialize);
            }
        }
    }
}
