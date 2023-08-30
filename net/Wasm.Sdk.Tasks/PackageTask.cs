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
    public partial class PackageTask : WasmTask
    {
        private static readonly char OtherDirectorySeparatorChar = Path.DirectorySeparatorChar == '/' ? '\\' : '/';

        [Required]
        public string IntermediateOutputPath { get; set; } = "";

        [Required]
        public string Assembly { get; set; } = "";

        [Required]
        public string RuntimeConfiguration { get; set; } = "";

        [Required]
        public string OutputDirectory { get; set; } = "";

        public ITaskItem[]? IncludeAssemblies { get; private set; }

        [Output]
        public ITaskItem[]? PackagedFiles { get; private set; }

        [Output]
        public string? IntermediatePackageDirectory { get; private set; }

        public override bool Execute()
        {
            try
            {
                IntermediatePackageDirectory = RunPackager();

                // Copy only pdbs that are directly used by the packaged project!
                var pdbs = Directory.EnumerateFiles(OutputDirectory, "*.pdb"); // Do not recurse search!
                foreach(var pdb in pdbs)
                {
                    var destinationPdb = Path.Combine(IntermediatePackageDirectory, Path.GetFileName(pdb));
                    File.Copy(pdb, destinationPdb, true);
                }

                // Generate output
                var deployedItems = Directory.EnumerateFiles(IntermediatePackageDirectory)
                    .Select(s => new TaskItem(s));

                PackagedFiles = deployedItems.ToArray();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString(), false, true, null);
            }

            return !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Align paths to fix issues with mixed path
        /// </summary>
        string FixupPath(string path)
            => path.Replace(OtherDirectorySeparatorChar, Path.DirectorySeparatorChar);

        private void DirectoryCreateDirectory(string directory)
        {
            var directoryName = FixupPath(directory);

            try
            {
                Directory.CreateDirectory(directoryName);
            }
            catch (Exception /*e*/)
            {
                Log.LogError($"Failed to create directory [{directoryName}][{directory}]");
                throw;
            }
        }

        private string GetAssemblySearchPathParameter()
        {
            var references = GetReferences();
            var referenceDirectories = references
                .Select(Path.GetDirectoryName)
                .Distinct();

            var searchPaths = referenceDirectories.Select(r => $"--search-path=\"{r}\"");
            var referencePathsParameter = string.Join(" ", searchPaths);

            return referencePathsParameter;
        }

        private string GetRootAssembliesParameter()
        {
            var rootAssemblies = new List<string>();

            // Vancho Fix
            // Ensure Javascrript Interop is added (Required by mono.wasm for C# invocation from js)
            rootAssemblies.Add($"\"{Path.GetFullPath(Assembly)}\"");

            var javascriptInteropLib = "System.Private.Runtime.InteropServices.JavaScript.dll";
            var javascriptInteropLibPath = Path.Combine(MonoWasmSDKPath, "runtimes", "browser-wasm", "lib", "net6.0", javascriptInteropLib);
            var javascriptInteropLibPathNormalized = Path.GetFullPath(javascriptInteropLibPath);

            if (IncludeAssemblies != null)
            {
                foreach (var includeAssembly in IncludeAssemblies)
                {
                    var assemblyName = includeAssembly.ToString();
                    if (string.IsNullOrEmpty(assemblyName) == false) 
                    {
                        if (assemblyName.EndsWith(".dll") == false)
                        {
                            assemblyName = assemblyName + ".dll";
                        }

                        var assemblyPath = Path.Combine(OutputDirectory, assemblyName);
                        if (File.Exists(assemblyPath))
                        {
                            var normalizedPath = Path.GetFullPath(assemblyPath);
                            rootAssemblies.Add($"\"{normalizedPath}\"");
                        }
                        else
                        {
                            Log.LogWarning($"The assembly {assemblyName} configured in <IncludeAssemblies> was not found in \"{OutputDirectory}\".");
                        }
                    }
                }
            }

            rootAssemblies.Add($"\"{javascriptInteropLibPathNormalized}\"");
            if (File.Exists(javascriptInteropLibPathNormalized) == false)
            {
                throw new ApplicationException($"{javascriptInteropLib} is a necessary reference!");
            }

            var rootAssembliesArgument = string.Join(" ", rootAssemblies);
            return rootAssembliesArgument;
        }

        private string CreateWorkDir()
        {
            var workDir = Path.Combine(IntermediateOutputPath, "_framework");
            if (Directory.Exists(workDir))
            {
                try
                {
                    Directory.Delete(workDir, true);
                }
                catch(Exception e)
                {
                    Log.LogError("Failed to clean intermediate directory");
                    Log.LogError(e.ToString());
                }
            }
            DirectoryCreateDirectory(workDir);

            return workDir;
        }

        private string RunPackager()
        {
            var workDir = CreateWorkDir();

            var monovmparams = $"--framework=net5 --runtimepack-dir={MonoWasmSDKPath} --icu -deploy=.";
            var referencePathsParameter = GetAssemblySearchPathParameter();
            var rootAssembliesArgument = GetRootAssembliesParameter();
            var packagerArgs = $"--runtime-config={RuntimeConfiguration} --appdir=\"{workDir}\" {monovmparams} --zlib {referencePathsParameter} {rootAssembliesArgument}";

            string packagerBinPath = Path.Combine(MonoWasmSDKPath, "tools", "packager.exe");
            var packagerResults = this.RunProcess(packagerBinPath, packagerArgs, workDir);

            if (packagerResults.exitCode != 0)
            {
                throw new Exception("Failed to generate wasm layout (More details are available in diagnostics mode or using the MSBuild /bl switch)");
            }

            return workDir;
        }
    }
}
