using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    public class TypeScriptProperty
    {
        public TypeScriptProperty(string name, WasmType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = Name.ToCamelCase();
        }

        public string Name { get; }
        public WasmType Type { get; }
    }

    public class TypeScriptEnumMember
    {
        public TypeScriptEnumMember(string memberName, int? memberValue)
        {
            Name = memberName ?? throw new ArgumentNullException(nameof(memberName));
            Value = memberValue;
            HasValue = memberValue != null;
        }

        public string Name { get; }
        public int? Value { get; }
        public bool HasValue { get; set; }
    }
}
