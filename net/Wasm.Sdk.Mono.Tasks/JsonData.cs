using System.Runtime.Serialization;
using IcuSection = Wasm.Tasks.Shared.ContentFile;
using WasmSection = Wasm.Tasks.Shared.ContentFile;
using TimeZoneSection = Wasm.Tasks.Shared.ContentFile;
using JSSection = Wasm.Tasks.Shared.ContentFile;

namespace Wasm.Sdk.Mono.Tasks
{
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
}
