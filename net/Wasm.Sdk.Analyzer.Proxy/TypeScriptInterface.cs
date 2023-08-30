using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    public class TypeScriptCodeBlock : IEquatable<TypeScriptCodeBlock>
    {
        public TypeScriptCodeBlock(string @namespace, string name)
        {
            Namespace = @namespace;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + Namespace.GetHashCode();
            hash = hash * 23 + Name.GetHashCode();

            return hash;
        }

        public string Namespace { get; }
        public string Name { get; }

        public bool Equals(TypeScriptCodeBlock other)
        {
            return Namespace.Equals(other.Namespace, StringComparison.InvariantCultureIgnoreCase)
                && Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
    public class TypeScriptInterface : TypeScriptCodeBlock, IEquatable<TypeScriptInterface>
    {
        public TypeScriptInterface(string @namespace, string name, IEnumerable<TypeScriptProperty> properties) : base(@namespace, name)
        {
            properties = properties ?? throw new ArgumentNullException(nameof(properties));

            Properties = new List<TypeScriptProperty>(properties);
        }
        public IReadOnlyList<TypeScriptProperty> Properties { get; }

        public bool Equals(TypeScriptInterface other)
        {
            return base.Equals(other);
        }
    }

    public class TypeScriptEnum : TypeScriptCodeBlock, IEquatable<TypeScriptEnum>
    {
        public TypeScriptEnum(string @namespace, string name, IEnumerable<TypeScriptEnumMember> members) : base(@namespace, name)
        {
            members = members ?? throw new ArgumentNullException(nameof(members));

            Members = new List<TypeScriptEnumMember>(members);
        }
        public IReadOnlyList<TypeScriptEnumMember> Members { get; }

        public bool Equals(TypeScriptEnum other)
        {
            return base.Equals(other);
        }
    }
}
