using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Wasm.Sdk.Tasks
{
    public abstract class WasmTask : Task
    {
        public ITaskItem[]? ReferencePath { get; set; }

        [Required]
        public string MonoWasmSDKPath { get; set; } = "";

        protected string BclPath => Path.GetFullPath(Path.Combine(MonoWasmSDKPath, "runtimes", "browser-wasm", "lib", "net6.0"));

        record BclInfo
        {
            public BclInfo(string path, Dictionary<string, string> assemblies)
            {
                this.Path = path;
                this.Assemblies = assemblies;
            }

            public string Path { get; }
            public Dictionary<string, string> Assemblies { get; }
        }

        private BclInfo GetBclData()
        {
            var bclPath = BclPath;

            var bclAssemblies = Directory
                .GetFiles(bclPath, "*.dll")
                .ToDictionary(x => Path.GetFileName(x));

            return new BclInfo(bclPath, bclAssemblies);
        }
        private static bool HasConcreteAssemblyForReferenceAssembly(ITaskItem other, ITaskItem referenceAssembly)
            => Path.GetFileName(other.ItemSpec) == Path.GetFileName(referenceAssembly.ItemSpec) && (other.GetMetadata("PathInPackage")?.StartsWith("lib/", StringComparison.OrdinalIgnoreCase) ?? false);

        protected IReadOnlyList<string> GetReferences()
        {
            var bcl = GetBclData();
            List<string> references = new();

            if (ReferencePath != null)
            {
                foreach (var referencePath in ReferencePath)
                {
                    var isReferenceAssembly = referencePath.GetMetadata("PathInPackage")?.StartsWith("ref/", StringComparison.OrdinalIgnoreCase) ?? false;
                    var hasConcreteAssembly = isReferenceAssembly && ReferencePath.Any(innerReference => HasConcreteAssemblyForReferenceAssembly(innerReference, referencePath));

                    if (isReferenceAssembly && hasConcreteAssembly)
                    {
                        // Reference assemblies may be present along with the actual assemblies.
                        // Filter out those assemblies as they cannot be used at runtime.
                        continue;
                    }

                    var name = Path.GetFileName(referencePath.ItemSpec);
                    if (
                        bcl.Assemblies.ContainsKey(name)

                        // NUnitLite is a particular case, as it is distributed
                        // as part of the mono runtime BCL, which prevents the nuget
                        // package from overriding it. We exclude it here, and cache the
                        // proper assembly in the resolver farther below, so that it gets 
                        // picked up first.
                        && name != "nunitlite.dll"
                    )
                    {
                        references.Add(bcl.Assemblies[name]);
                    }
                    else
                    {
                        references.Add(referencePath.ItemSpec);
                    }
                }
            }

            return references;
        }
    }
}
