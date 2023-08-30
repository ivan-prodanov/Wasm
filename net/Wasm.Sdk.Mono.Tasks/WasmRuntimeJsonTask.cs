using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using IcuSection = Wasm.Tasks.Shared.ContentFile;
using WasmSection = Wasm.Tasks.Shared.ContentFile;
using TimeZoneSection = Wasm.Tasks.Shared.ContentFile;
using JSSection = Wasm.Tasks.Shared.ContentFile;
using Wasm.Tasks.Shared;

namespace Wasm.Sdk.Mono.Tasks
{
    // Token: 0x0200000B RID: 11
    public class WasmRuntimeJsonTask : Task
    {
        private const string RuntimeFileName = "wasm.runtime.json";

        [Required]
        public ITaskItem[]? IcuFiles { get; set; }

        [Required]
        public ITaskItem? WasmFile { get; set; }

        [Required]
        public ITaskItem? JsFile { get; set; }

        [Required]
        public ITaskItem? TimeZoneFile { get; set; }

        [Required]
        public bool DebugBuild { get; set; }

        [Required]
        public string? OutputPath { get; set; }

        public bool LoadAllICUData { get; set; }

        public bool InvariantGlobalization { get; set; }

        public override bool Execute()
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }
            Directory.CreateDirectory(OutputPath);

            var filePath = Path.Combine(OutputPath, RuntimeFileName);
            using var fileStream = File.Create(filePath);

            try
            {
                WriteBootJson(fileStream);
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

        public void WriteBootJson(Stream output)
        {
            ContentFile GetContentFile(ITaskItem taskItem)
            {
                var fileName = taskItem.GetMetadata("FileName");
                var extension = taskItem.GetMetadata("Extension");
                var file = $"{fileName}{extension}";
                var fullPath = taskItem.GetMetadata("FullPath");

                var contentFile = new ContentFile(file, GetSize(fullPath), GetHash(taskItem));
                return contentFile;
            }

            var icuDataMode = ICUDataMode.Sharded;

            if (InvariantGlobalization)
            {
                icuDataMode = ICUDataMode.Invariant;
            }
            else if (LoadAllICUData)
            {
                icuDataMode = ICUDataMode.All;
            }

            if (WasmFile == null)
            {
                Log.LogError("Wasm file is not set");
                return;
            }

            if (JsFile == null)
            {
                Log.LogError("Js file is not set");
                return;
            }

            if (TimeZoneFile == null)
            {
                Log.LogError("TimeZone file is not set");
                return;
            }

            if (IcuFiles == null)
            {
                Log.LogError("Icu files are not set");
                return;
            }

            var wasm = GetContentFile(WasmFile);
            var js = GetContentFile(JsFile);
            var timeZone = GetContentFile(TimeZoneFile);

            IcuSection[] icuSections = IcuFiles
                .Select(GetContentFile)
                .ToArray();

            var bootConfiguration = new WasmRuntime(DebugBuild, icuDataMode, wasm, js, icuSections, timeZone);

            var serializer = new DataContractJsonSerializer(typeof(WasmRuntime), new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            });

            using var writer = JsonReaderWriterFactory.CreateJsonWriter(output, Encoding.UTF8, ownsStream: false, indent: true);
            serializer.WriteObject(writer, bootConfiguration);
        }
    }
}
