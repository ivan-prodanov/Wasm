using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    class WasmClass
    {
        public WasmClass(string assemblyName, string @namespace, string name, IEnumerable<WasmConstructor> constructors, IEnumerable<WasmMethod> methods)
        {
            AssemblyName = assemblyName;
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ProxyName = $"_{Name}";
            constructors = constructors ?? throw new ArgumentNullException(nameof(constructors));
            methods = methods ?? throw new ArgumentNullException(nameof(methods));
            Constructors = new List<WasmConstructor>(constructors);
            Methods = new List<WasmMethod>(methods);

            for (var i = 0; i < Constructors.Count; i++)
            {
                Constructors[i].ProxyName = $"{name}_wctor{i + 1}";
            }
        }

        public string AssemblyName { get; }
        public string Namespace { get; }
        public string Name { get; }
        public string ProxyName { get; }
        public List<WasmConstructor> Constructors { get; }
        public List<WasmMethod> Methods { get; }

        public string FQN => $"{Namespace}.{Name}";
        public string ProxyFQN => $"{Namespace}.{Name}";
    }
}
