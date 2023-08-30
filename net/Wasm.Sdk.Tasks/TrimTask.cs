using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Wasm.Sdk.Tasks
{
    public class TrimTask : WasmTask
    {
        [Required]
        public string IntermediateOutputPath { get; set; } = "";

        [Required]
        public string Assembly { get; set; } = "";

        [Output]
        public ITaskItem[]? TrimmedFiles { get; private set; }

        [Output]
        public string? IntermediateTrimDirectory { get; private set; }

        public override bool Execute()
        {
            try
            {
                IntermediateTrimDirectory = Path.Combine(IntermediateOutputPath, "Trimmed");
                if (Directory.Exists(IntermediateTrimDirectory))
                {
                    Directory.Delete(IntermediateTrimDirectory, true);
                }
                Directory.CreateDirectory(IntermediateTrimDirectory);

                Trim(IntermediateTrimDirectory);

                // Generate output
                var deployedItems = Directory.EnumerateFiles(IntermediateTrimDirectory)
                    .Select(s => new TaskItem(s));

                TrimmedFiles = deployedItems.ToArray();
            }
            catch (Exception e)
            {
                Log.LogError($"Trimming interrupted with error: {e}");
            }

            return !Log.HasLoggedErrors;
        }

        private void Trim(string outputDirectory)
        {
            var referencedAssemblies = GetReferences();
            var linkerBinPath = Path.Combine(MonoWasmSDKPath, "tools", "illink.dll");

            var assemblyPath = Path.Combine(IntermediateOutputPath, Path.GetFileName(Assembly));

            var frameworkBindings = new List<string>();

            frameworkBindings.Add("System.Private.Runtime.InteropServices.JavaScript.dll");

            var linkerSearchPaths = string.Join(" ", referencedAssemblies.Select(Path.GetDirectoryName).Distinct().Select(p => $"-d \"{p}\" "));
            var fullSDKFolder = $"-d \"{BclPath}\" {linkerSearchPaths}";

            var bindingsPath = string.Join(" ", frameworkBindings.Select(a => $"-a \"{Path.Combine(IntermediateOutputPath, a)}\""));

            // Opts should be aligned with the monolinker call in packager.cs, validate for linker_args as well
            var packagerLinkerOpts = $"--deterministic --disable-opt unreachablebodies --used-attrs-only true ";

            var linkerResults = this.RunProcess(
                linkerBinPath,
                $"-out \"{outputDirectory}\" --verbose -b true {packagerLinkerOpts} -a \"{assemblyPath}\" {bindingsPath} -c link -p copy \"WebAssembly.Bindings\" -d \"{IntermediateOutputPath}\" {fullSDKFolder}",
                outputDirectory
            );

            if (linkerResults.exitCode != 0)
            {
                throw new Exception("Failed to execute the linker");
            }
        }
    }
}
