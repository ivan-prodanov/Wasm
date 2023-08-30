using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    public class WasmParameter
    {
        public WasmParameter(string name, WasmType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            type = type ?? throw new ArgumentNullException(nameof(type));
            Type = type;
            TsParam = $"{Name}: {Type.TsType}";
            ManagedParam = $"{Type.ProxyType} {Name}";

            switch (type.TypeConversion)
            {
                case TypeConversion.Serialize:
                    ManagedName = $"Wasm.Sdk.JsonSerializationHelper.Deserialize<{type.ManagedType}>({name}, nameof({name}))";
                    TsName = $"JSON.stringify({name})";
                    break;
                case TypeConversion.TypedArray:
                    ManagedName = $"{name}.ToArray()";
                    TsName = name;
                    break;
                case TypeConversion.None:
                    ManagedName = name;
                    TsName = name;
                    break;
                default:
                    throw new NotSupportedException($"{type.TypeConversion} is not supported yet!");
            }
        }

        public string Name { get; }
        public string ManagedName { get; }
        public WasmType Type { get; set; }
        public string TsName { get; }

        public string TsParam { get; }
        public string ManagedParam { get; }
    }
}
