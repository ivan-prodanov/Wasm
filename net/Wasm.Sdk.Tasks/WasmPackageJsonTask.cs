using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PdbSection = Wasm.Tasks.Shared.ContentFile;
using DocumentationSection = Wasm.Tasks.Shared.ContentFile;
using NativeFileSection = Wasm.Tasks.Shared.ContentFile;
using Wasm.Tasks.Shared;

namespace Wasm.Sdk.Tasks
{
    // Token: 0x0200000B RID: 11
    public class WasmPackageJsonTask : Task
	{
        private const string PackageFileName = "wasm.package.json";
        private Dictionary<string, string> assemblyDocumentationRedirects = new Dictionary<string, string>
        {
            ["mscorlib"] = "System.Private.CoreLib"
        };

        [Required]
        public string? Name { get; set; }
        public string? PackageName { get; set; }
        [Required]
        public string? AssemblyPath { get; set; }

        [Required]
        public ITaskItem[]? Resources { get; set; }

        [Required]
        public string? OutputPath { get; set; }

        public override bool Execute()
        {
            var packageFileName = Path.Combine(OutputPath, PackageFileName);
            using var fileStream = File.Create(packageFileName);
            var entryAssemblyName = AssemblyName.GetAssemblyName(AssemblyPath).Name;
            var packageName = PackageName ?? NpmPackageJsonTask.ConvertToPackageName(Name);

            try
            {
                WriteBootJson(fileStream, entryAssemblyName, packageName ?? "");
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }

        private string GetHash(ITaskItem item)
            => $"sha256-{item.GetMetadata("FileHash")}";

        private long GetSize(string path)
            => new FileInfo(path).Length;

        private string GetVersion(string filePath)
            => AssemblyName.GetAssemblyName(filePath).Version.ToString();

        public void WriteBootJson(Stream output, string entryAssemblyName, string packageId)
        {
            if (Resources == null || string.IsNullOrEmpty(packageId))
            {
                throw new InvalidOperationException("There are no resources assigned");
            }

            var sateliteAssemblies = new Dictionary<string, List<ContentFile>>();
            var pdbs = new Dictionary<string, PdbSection>();
            var docs = new Dictionary<string, DocumentationSection>();
            var assemblies = new Dictionary<string, AssemblySection>();
            var nativeFiles = new List<NativeFileSection>();

            foreach (var resource in Resources)
            {
                var fileName = resource.GetMetadata("FileName");
                var extension = resource.GetMetadata("Extension");
                var resourceCulture = resource.GetMetadata("Culture");
                var assetType = resource.GetMetadata("AssetType");
                var resourceName = $"{fileName}{extension}";
                var fullPath = resource.GetMetadata("FullPath");

                if (!string.IsNullOrEmpty(resourceCulture))
                {
                    if (sateliteAssemblies.TryGetValue(resourceCulture, out var assemblyConfig) == false)
                    {
                        assemblyConfig = new List<ContentFile>();
                        sateliteAssemblies[resourceCulture] = assemblyConfig;
                    }

                    assemblyConfig.Add(new ContentFile(resourceName, GetSize(fullPath), GetHash(resource)));
                }
                else if (string.Equals(extension, ".pdb", StringComparison.OrdinalIgnoreCase))
                {
                    pdbs[fileName] = new PdbSection(resourceName, GetSize(fullPath), GetHash(resource));
                }
                else if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
                {
                    assemblies[fileName] = new AssemblySection(resourceName, GetVersion(fullPath), GetSize(fullPath), GetHash(resource));
                }
                else if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    docs[fileName] = new DocumentationSection(resourceName, GetSize(fullPath), GetHash(resource));
                }
                else if (string.Equals(assetType, "native", StringComparison.OrdinalIgnoreCase))
                {
                    nativeFiles.Add(new NativeFileSection(resourceName, GetSize(fullPath), GetHash(resource)));
                }
            }

            foreach (var pdbPair in pdbs)
            {
                if (assemblies.TryGetValue(pdbPair.Key, out var assembly))
                {
                    assembly.Pdb = pdbPair.Value;
                }
            }

            foreach (var docPair in docs)
            {
                assemblyDocumentationRedirects.TryGetValue(docPair.Key, out var targetKey);
                targetKey = targetKey ?? docPair.Key;

                if (assemblies.TryGetValue(targetKey, out var assembly))
                {
                    assembly.Doc = docPair.Value;
                }
            }

            var localizedAssemblies = sateliteAssemblies
                .Select(localizedCollection => new LocalizedAssemblies(localizedCollection.Key, localizedCollection.Value.ToArray()))
                .ToArray();

            var bootConfiguration = new WasmPackage(packageId, entryAssemblyName, assemblies.Values.ToArray(), localizedAssemblies, nativeFiles.ToArray());

            var serializer = new DataContractJsonSerializer(typeof(WasmPackage), new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            });

            using var writer = JsonReaderWriterFactory.CreateJsonWriter(output, Encoding.UTF8, ownsStream: false, indent: true);
            serializer.WriteObject(writer, bootConfiguration);
        }
    }
}
