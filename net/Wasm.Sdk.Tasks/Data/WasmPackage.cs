using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PdbSection = Wasm.Tasks.Shared.ContentFile;
using DocumentationSection = Wasm.Tasks.Shared.ContentFile;
using NativeFileSection = Wasm.Tasks.Shared.ContentFile;
using IcuSection = Wasm.Tasks.Shared.ContentFile;
using WasmSection = Wasm.Tasks.Shared.ContentFile;
using TimeZoneSection = Wasm.Tasks.Shared.ContentFile;
using JSSection = Wasm.Tasks.Shared.ContentFile;
using Wasm.Tasks.Shared;

namespace Wasm.Sdk.Tasks
{
    // wasm.package.json
    [DataContract]
    public class WasmPackage
    {
        public WasmPackage(string id, string entryAssembly, AssemblySection[] assemblies, LocalizedAssemblies[] satelliteResources, NativeFileSection[] nativeFiles)
        {
            this.Id = id;
            this.EntryAssembly = entryAssembly;
            this.Assemblies = assemblies;
            this.SatelliteResources = satelliteResources.Length != 0 ? satelliteResources : null;
            this.NativeFiles = nativeFiles.Length != 0 ? nativeFiles : null;
        }

        [DataMember(Order = 0, Name = "id")]
        public string Id { get; set; }

        [DataMember(Order = 1, Name = "name")]
        public string EntryAssembly { get; set; }

        [DataMember(Order = 2, Name = "assemblies")]
        public AssemblySection[] Assemblies { get; set; }

        [DataMember(Order = 3, Name = "satelliteResources", EmitDefaultValue = false)]
        public LocalizedAssemblies[]? SatelliteResources { get; set; }

        [DataMember(Order = 4, Name = "nativeFiles", EmitDefaultValue = false)]
        public NativeFileSection[]? NativeFiles { get; set; }
    }

    // wasm.runtime.json
    [DataContract]
    public class WasmRuntime
    {
        public WasmRuntime(bool debugBuild, ICUDataMode icuDataMode, WasmSection wasm, JSSection js, IcuSection[] icuCollection, TimeZoneSection timeZone)
        {
            DebugBuild = debugBuild;
            IcuDataMode = icuDataMode;
            Wasm = wasm;
            Js = js;
            IcuCollection = icuCollection;
            TimeZone = timeZone;
        }

        [DataMember(Order = 0, Name = "debugBuild")]
        public bool DebugBuild { get; set; }

        [DataMember(Order = 1, Name = "icuDataMode")]
        public ICUDataMode IcuDataMode;

        [DataMember(Order = 2, Name = "wasm")]
        public WasmSection Wasm { get; set; }

        [DataMember(Order = 3, Name = "js")]
        public JSSection Js { get; set; }

        [DataMember(Order = 4, Name = "icu")]
        public IcuSection[] IcuCollection { get; set; }

        [DataMember(Order = 5, Name = "tz")]
        public TimeZoneSection TimeZone { get; set; }
    }

    public enum ICUDataMode
    {
        Sharded,
        All,
        Invariant
    }

    [DataContract]
    public class LocalizedAssemblies
    {
        public LocalizedAssemblies(string culture, ContentFile[] assemblies)
        {
            this.Culture = culture;
            this.Assemblies = assemblies;
        }

        [DataMember(Order = 0, Name = "culture")]
        public string Culture { get; set; }

        [DataMember(Order = 1, Name = "assemblies")]
        public ContentFile[] Assemblies { get; set; }
    }

    [DataContract]
    public class AssemblySection : ContentFile
    {
		public AssemblySection(string name, string version, long size, string hash) : base(name, size, hash)
        {
            this.Version = version;
        }

        [DataMember(Order = 3, Name = "version")]
        public string Version { get; set; }

        [DataMember(Order = 4, Name = "pdb", EmitDefaultValue = false)]
        public PdbSection? Pdb { get; set; }

        [DataMember(Order = 5, Name = "doc", EmitDefaultValue = false)]
        public DocumentationSection? Doc { get; set; }
    }
}
