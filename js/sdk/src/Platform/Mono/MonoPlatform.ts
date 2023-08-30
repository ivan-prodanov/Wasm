import {
  WebAssemblyResourceLoader,
  LoadingResource,
} from "../WebAssemblyResourceLoader";
import {
  Platform,
  System_Array,
  Pointer,
  System_Object,
  System_String,
  HeapLock,
} from "..";
import { WebAssemblyBootResourceType } from "../WebAssemblyStartOptions";
import { BootJsonData, ICUDataMode } from "../BootConfig";
import { toAbsoluteUri } from "../../Services/NavigationManager";

let mono_wasm_add_assembly: (
  name: string,
  heapAddress: number,
  length: number
) => void;
const appBinDirName = "appBinDir";
const uint64HighOrderShift = Math.pow(2, 32);
const maxSafeNumberHighPart = Math.pow(2, 21) - 1; // The high-order int32 from Number.MAX_SAFE_INTEGER

let currentHeapLock: MonoHeapLock | null = null;

// Memory access helpers
// The implementations are exactly equivalent to what the global getValue(addr, type) function does,
// except without having to parse the 'type' parameter, and with less risk of mistakes at the call site
function getValueI16(ptr: number) {
  return Module.HEAP16[ptr >> 1];
}
function getValueI32(ptr: number) {
  return Module.HEAP32[ptr >> 2];
}
function getValueFloat(ptr: number) {
  return Module.HEAPF32[ptr >> 2];
}
function getValueU64(ptr: number) {
  // There is no Module.HEAPU64, and Module.getValue(..., 'i64') doesn't work because the implementation
  // treats 'i64' as being the same as 'i32'. Also we must take care to read both halves as unsigned.
  const heapU32Index = ptr >> 2;
  const highPart = Module.HEAPU32[heapU32Index + 1];
  if (highPart > maxSafeNumberHighPart) {
    throw new Error(
      `Cannot read uint64 with high order part ${highPart}, because the result would exceed Number.MAX_SAFE_INTEGER.`
    );
  }

  return highPart * uint64HighOrderShift + Module.HEAPU32[heapU32Index];
}

export const monoPlatform: Platform = {
  start: function start(
    resourceLoader: WebAssemblyResourceLoader,
    pathResolver: (path: string) => string,
    onConsoleLog: (text: string) => void,
    onConsoleErr: (text: string) => void,
    onRuntimeErr: (text: string) => void
  ) {
    return new Promise<void>((resolve, reject) => {
      // dotnet.js assumes the existence of this
      self["Browser"] = {
        init: () => {},
      };

      // Emscripten works by expecting the module config to be a global
      // For compatibility with macOS Catalina, we have to assign a temporary value to self.Module
      // before we start loading the WebAssembly files
      addGlobalModuleScriptTagsToDocument(() => {
        self["Module"] = createEmscriptenModuleInstance(
          resourceLoader,
          pathResolver,
          resolve,
          reject,
          onConsoleLog,
          onConsoleErr,
          onRuntimeErr
        );
        addScriptTagsToDocument(resourceLoader, pathResolver);
      });
    });
  },

  toUint8Array: function toUint8Array(array: System_Array<any>): Uint8Array {
    const dataPtr = getArrayDataPointer(array);
    const length = getValueI32(dataPtr);
    return new Uint8Array(Module.HEAPU8.buffer, dataPtr + 4, length);
  },

  getArrayLength: function getArrayLength(array: System_Array<any>): number {
    return getValueI32(getArrayDataPointer(array));
  },

  getArrayEntryPtr: function getArrayEntryPtr<TPtr extends Pointer>(
    array: System_Array<TPtr>,
    index: number,
    itemSize: number
  ): TPtr {
    // First byte is array length, followed by entries
    const address = getArrayDataPointer(array) + 4 + index * itemSize;
    return address as any as TPtr;
  },

  getObjectFieldsBaseAddress: function getObjectFieldsBaseAddress(
    referenceTypedObject: System_Object
  ): Pointer {
    // The first two int32 values are internal Mono data
    return ((referenceTypedObject as any as number) + 8) as any as Pointer;
  },

  readInt16Field: function readHeapInt16(
    baseAddress: Pointer,
    fieldOffset?: number
  ): number {
    return getValueI16((baseAddress as any as number) + (fieldOffset || 0));
  },

  readInt32Field: function readHeapInt32(
    baseAddress: Pointer,
    fieldOffset?: number
  ): number {
    return getValueI32((baseAddress as any as number) + (fieldOffset || 0));
  },

  readUint64Field: function readHeapUint64(
    baseAddress: Pointer,
    fieldOffset?: number
  ): number {
    return getValueU64((baseAddress as any as number) + (fieldOffset || 0));
  },

  readFloatField: function readHeapFloat(
    baseAddress: Pointer,
    fieldOffset?: number
  ): number {
    return getValueFloat((baseAddress as any as number) + (fieldOffset || 0));
  },

  readObjectField: function readHeapObject<T extends System_Object>(
    baseAddress: Pointer,
    fieldOffset?: number
  ): T {
    return getValueI32(
      (baseAddress as any as number) + (fieldOffset || 0)
    ) as any as T;
  },

  readStringField: function readHeapObject(
    baseAddress: Pointer,
    fieldOffset?: number,
    readBoolValueAsString?: boolean
  ): string | null {
    const fieldValue = getValueI32(
      (baseAddress as any as number) + (fieldOffset || 0)
    );
    if (fieldValue === 0) {
      return null;
    }

    if (readBoolValueAsString) {
      // Some fields are stored as a union of bool | string | null values, but need to read as a string.
      // If the stored value is a bool, the behavior we want is empty string ('') for true, or null for false.
      const unboxedValue = BINDING.unbox_mono_obj(
        fieldValue as any as System_Object
      );
      if (typeof unboxedValue === "boolean") {
        return unboxedValue ? "" : null;
      }
      return unboxedValue;
    }

    let decodedString: string | null | undefined;
    if (currentHeapLock) {
      decodedString = currentHeapLock.stringCache.get(fieldValue);
      if (decodedString === undefined) {
        decodedString = BINDING.conv_string(fieldValue as any as System_String);
        currentHeapLock.stringCache.set(fieldValue, decodedString);
      }
    } else {
      decodedString = BINDING.conv_string(fieldValue as any as System_String);
    }

    return decodedString;
  },

  readStructField: function readStructField<T extends Pointer>(
    baseAddress: Pointer,
    fieldOffset?: number
  ): T {
    return ((baseAddress as any as number) + (fieldOffset || 0)) as any as T;
  },

  beginHeapLock: function () {
    assertHeapIsNotLocked();
    currentHeapLock = new MonoHeapLock();
    return currentHeapLock;
  },

  invokeWhenHeapUnlocked: function (callback) {
    // This is somewhat like a sync context. If we're not locked, just pass through the call directly.
    if (!currentHeapLock) {
      callback();
    } else {
      currentHeapLock.enqueuePostReleaseAction(callback);
    }
  },
};

function addScriptTagsToDocument(
  resourceLoader: WebAssemblyResourceLoader,
  pathResolver: (path: string) => string
) {
  const browserSupportsNativeWebAssembly =
    typeof WebAssembly !== "undefined" && WebAssembly.validate;
  if (!browserSupportsNativeWebAssembly) {
    throw new Error("This browser does not support WebAssembly.");
  }
  const dotnetJsResource = resourceLoader.bootConfig.js;
  let dotnetJsPath = pathResolver(`runtime\\${dotnetJsResource.name}`);

  if (self.document) {
    // The dotnet.*.js file has a version or hash in its name as a form of cache-busting. This is needed
    // because it's the only part of the loading process that can't use cache:'no-cache' (because it's
    // not a 'fetch') and isn't controllable by the developer (so they can't put in their own cache-busting
    // querystring). So, to find out the exact URL we have to search the boot manifest.
    const scriptElem = self.document.createElement("script");
    scriptElem.src = dotnetJsPath;
    scriptElem.defer = true;

    // For consistency with WebAssemblyResourceLoader, we only enforce SRI if caching is allowed
    if (resourceLoader.bootConfig.cacheBootResources) {
      // VANCHO disables hash check
      scriptElem.integrity = dotnetJsResource.hash;
      scriptElem.crossOrigin = "anonymous";
    }

    // Allow overriding the URI from which the dotnet.*.js file is loaded
    if (resourceLoader.startOptions.loadBootResource) {
      const resourceType: WebAssemblyBootResourceType = "dotnetjs";
      const customSrc = resourceLoader.startOptions.loadBootResource(
        resourceType,
        dotnetJsResource.name,
        scriptElem.src,
        dotnetJsResource.hash
      );
      if (typeof customSrc === "string") {
        scriptElem.src = customSrc;
      } else if (customSrc) {
        // Since we must load this via a <script> tag, it's only valid to supply a URI (and not a Request, say)
        throw new Error(
          `For a ${resourceType} resource, custom loaders must supply a URI string.`
        );
      }
    }

    self.document.body.appendChild(scriptElem);
  } else {
    // Allow overriding the URI from which the dotnet.*.js file is loaded
    if (resourceLoader.startOptions.loadBootResource) {
      const resourceType: WebAssemblyBootResourceType = "dotnetjs";
      const customSrc = resourceLoader.startOptions.loadBootResource(
        resourceType,
        dotnetJsResource.name,
        dotnetJsPath,
        dotnetJsResource.hash
      );
      if (typeof customSrc === "string") {
        dotnetJsPath = customSrc;
      } else if (customSrc) {
        // Since we must load this via a <script> tag, it's only valid to supply a URI (and not a Request, say)
        throw new Error(
          `For a ${resourceType} resource, custom loaders must supply a URI string.`
        );
      }
    }

    self.importScripts(dotnetJsPath);
  }
}

// Due to a strange behavior in macOS Catalina, we have to delay loading the WebAssembly files
// until after it finishes evaluating a <script> element that assigns a value to self.Module.
// This may be fixed in a later version of macOS/iOS, or even if not it may be possible to reduce
// this to a smaller workaround.
function addGlobalModuleScriptTagsToDocument(callback: () => void) {
  if (self.document) {
    const scriptElem = self.document.createElement("script");

    // This pollutes global but is needed so it can be called from the script.
    // The callback is put in the global scope so that it can be run after the script is loaded.
    // onload cannot be used in this case for non-file scripts.
    self["__wasmmodulecallback__"] = callback;
    scriptElem.type = "text/javascript";
    scriptElem.text =
      "var Module; self.__wasmmodulecallback__(); delete self.__wasmmodulecallback__;";

    self.document.body.appendChild(scriptElem);
  } else {
    callback();
  }
}

function createEmscriptenModuleInstance(
  resourceLoader: WebAssemblyResourceLoader,
  pathResolver: (path: string) => string,
  onReady: () => void,
  onError: (reason?: any) => void,
  onConsoleLog: (text: string) => void,
  onConsoleErr: (text: string) => void,
  onRuntimeErr: (text: string) => void
) {
  const resources = resourceLoader.bootConfig;
  const selectedPackages = resourceLoader.startOptions.packages;

  const module = (self["Module"] || {}) as typeof Module;
  const suppressMessages = ["DEBUGGING ENABLED"];

  module.print = (line) =>
    suppressMessages.indexOf(line) < 0 && onConsoleLog(line);

  module.printErr = (line) => {
    onConsoleErr(line);
  };
  module.preRun = module.preRun || [];
  module.postRun = module.postRun || [];
  (module as any).preloadPlugins = [];

  let assembliesBeingLoaded: LoadingResource[] = [];
  let pdbsBeingLoaded: LoadingResource[] = [];
  const targetedPackages = resources.packages.filter(
    (p) => selectedPackages?.indexOf(p.id) !== -1
  );

  for (let pkg of targetedPackages) {
    // Begin loading the .dll/.pdb/.wasm files, but don't block here. Let other loading processes run in parallel.
    assembliesBeingLoaded.push.apply(
      assembliesBeingLoaded,
      resourceLoader.loadResources(
        pkg.assemblies,
        (filename) => pathResolver(`${pkg.id}\\${filename}`),
        "assembly"
      )
    );
    pdbsBeingLoaded.push.apply(
      pdbsBeingLoaded,
      Array.from(
        resourceLoader.loadPdbResource(pkg.assemblies || {}, (filename) =>
          pathResolver(`${pkg.id}\\${filename}`)
        )
      )
    );
  }

  const wasmBeingLoaded = resourceLoader.loadResource(
    /* name */ resources.wasm.name,
    /* url */ pathResolver(`runtime\\${resources.wasm.name}`),
    /* hash */ resources.wasm.hash,
    /* size */ resources.wasm.size,
    /* type */ "dotnetwasm"
  );

  let timeZoneResource: LoadingResource | undefined;
  if (resourceLoader.bootConfig.tz) {
    timeZoneResource = resourceLoader.loadResource(
      resourceLoader.bootConfig.tz.name,
      pathResolver(`runtime\\${resourceLoader.bootConfig.tz.name}`),
      resourceLoader.bootConfig.tz.hash,
      resourceLoader.bootConfig.tz.size,
      "globalization"
    );
  }

  let icuDataResource: LoadingResource | undefined;
  if (resourceLoader.bootConfig.icuDataMode != ICUDataMode.Invariant) {
    const applicationCulture =
      resourceLoader.startOptions.applicationCulture ||
      (navigator.languages && navigator.languages[0]);
    const icuDataResourceName = getICUResourceName(
      resourceLoader.bootConfig,
      applicationCulture
    );
    let icuFile = resourceLoader.bootConfig.icu.find(
      ({ name }) => name == icuDataResourceName
    );
    if (icuFile) {
      icuDataResource = resourceLoader.loadResource(
        icuFile.name,
        pathResolver(`runtime\\${icuFile.name}`),
        icuFile.hash,
        icuFile.size,
        "globalization"
      );
    }
  }

  // Override the mechanism for fetching the main wasm file so we can connect it to our cache
  module.instantiateWasm = (
    imports,
    successCallback
  ): Emscripten.WebAssemblyExports => {
    (async () => {
      let compiledInstance: WebAssembly.Instance;
      try {
        const dotnetWasmResource = await wasmBeingLoaded;
        compiledInstance = await compileWasmModule(dotnetWasmResource, imports);
      } catch (ex) {
        module.printErr(ex as any);
        throw ex;
      }
      successCallback(compiledInstance);
    })();
    return []; // No exports
  };

  module.preRun.push(() => {
    // By now, emscripten should be initialised enough that we can capture these methods for later use
    mono_wasm_add_assembly = cwrap("mono_wasm_add_assembly", null, [
      "string",
      "number",
      "number",
    ]);
    MONO.loaded_files = [];

    if (timeZoneResource) {
      loadTimezone(timeZoneResource);
    }

    if (icuDataResource) {
      loadICUData(icuDataResource);
    } else {
      // Use invariant culture if the app does not carry icu data.
      MONO.mono_wasm_setenv("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1");
    }

    // Fetch the assemblies and PDBs in the background, telling Mono to wait until they are loaded
    // Mono requires the assembly filenames to have a '.dll' extension, so supply such names regardless
    // of the extensions in the URLs. This allows loading assemblies with arbitrary filenames.
    assembliesBeingLoaded.forEach((r) =>
      addResourceAsAssembly(r, changeExtension(r.name, ".dll"))
    );
    pdbsBeingLoaded.forEach((r) => addResourceAsAssembly(r, r.name));

    self["WasmSdk"]._internal.dotNetCriticalError = (
      message: System_String
    ) => {
      onRuntimeErr(BINDING.conv_string(message) || "(null)");
    };
  });

  module.postRun.push(() => {
    if (
      resourceLoader.bootConfig.debugBuild &&
      resourceLoader.bootConfig.cacheBootResources
    ) {
      resourceLoader.logToConsole();
    }
    resourceLoader.purgeUnusedCacheEntriesAsync(); // Don't await - it's fine to run in background

    if (resourceLoader.bootConfig.icuDataMode === ICUDataMode.Sharded) {
      MONO.mono_wasm_setenv("__SHARDED_ICU", "1");

      if (resourceLoader.startOptions.applicationCulture) {
        // If a culture is specified via start options use that to initialize the Emscripten \  .NET culture.
        MONO.mono_wasm_setenv(
          "LANG",
          `${resourceLoader.startOptions.applicationCulture}.UTF-8`
        );
      }
    }
    MONO.mono_wasm_setenv("MONO_URI_DOTNETRELATIVEORABSOLUTE", "true");
    let timeZone = "UTC";
    try {
      timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    } catch {}
    MONO.mono_wasm_setenv("TZ", timeZone || "UTC");
    // Turn off full-gc to prevent browser freezing.
    const mono_wasm_enable_on_demand_gc = cwrap(
      "mono_wasm_enable_on_demand_gc",
      null,
      ["number"]
    );
    mono_wasm_enable_on_demand_gc(0);
    const load_runtime = cwrap("mono_wasm_load_runtime", null, [
      "string",
      "number",
    ]);
    // -1 enables debugging with logging disabled. 0 disables debugging entirely.
    load_runtime(appBinDirName, resourceLoader.bootConfig.debugBuild ? -1 : 0);
    MONO.mono_wasm_runtime_ready();
    // VANCHO KOLIO check this and continue cleaning
    //attachInteropInvoker();
    onReady();
  });

  return module;

  async function addResourceAsAssembly(
    dependency: LoadingResource,
    loadAsName: string
  ) {
    const runDependencyId = `wasmsdk:${dependency.name}`;
    addRunDependency(runDependencyId);

    try {
      // Wait for the data to be loaded and verified
      const dataBuffer = await dependency.response.then((r) => r.arrayBuffer());

      // Load it into the Mono runtime
      const data = new Uint8Array(dataBuffer);
      const heapAddress = Module._malloc(data.length);
      const heapMemory = new Uint8Array(
        Module.HEAPU8.buffer,
        heapAddress,
        data.length
      );
      heapMemory.set(data);
      mono_wasm_add_assembly(loadAsName, heapAddress, data.length);
      MONO.loaded_files.push(toAbsoluteUri(dependency.url));
    } catch (errorInfo) {
      onError(errorInfo);
      return;
    }

    removeRunDependency(runDependencyId);
  }
}

// const anchorTagForAbsoluteUrlConversions = document.createElement("a");
// function toAbsoluteUrl(possiblyRelativeUrl: string) {
//   anchorTagForAbsoluteUrlConversions.href = possiblyRelativeUrl;
//   return anchorTagForAbsoluteUrlConversions.href;
// }

function getArrayDataPointer<T>(array: System_Array<T>): number {
  return <number>(<any>array) + 12; // First byte from here is length, then following bytes are entries
}

function bindStaticMethod(assembly: string, typeName: string, method: string) {
  // Fully qualified name looks like this: "[debugger-test] Math:IntAdd"
  const fqn = `[${assembly}] ${typeName}:${method}`;
  return BINDING.bind_static_method(fqn);
}

async function loadTimezone(timeZoneResource: LoadingResource): Promise<void> {
  const runDependencyId = `wasmsdk:timezonedata`;
  addRunDependency(runDependencyId);

  const request = await timeZoneResource.response;
  const arrayBuffer = await request.arrayBuffer();

  Module["FS_createPath"]("/", "usr", true, true);
  Module["FS_createPath"]("/usr/", "share", true, true);
  Module["FS_createPath"]("/usr/share/", "zoneinfo", true, true);
  MONO.mono_wasm_load_data_archive(
    new Uint8Array(arrayBuffer),
    "/usr/share/zoneinfo/"
  );

  removeRunDependency(runDependencyId);
}

function getICUResourceName(
  bootConfig: BootJsonData,
  culture: string | undefined
): string {
  const combinedICUResourceName = "icudt.dat";
  if (!culture || bootConfig.icuDataMode === ICUDataMode.All) {
    return combinedICUResourceName;
  }

  const prefix = culture.split("-")[0];
  if (["en", "fr", "it", "de", "es"].includes(prefix)) {
    return "icudt_EFIGS.dat";
  } else if (["zh", "ko", "ja"].includes(prefix)) {
    return "icudt_CJK.dat";
  } else {
    return "icudt_no_CJK.dat";
  }
}

async function loadICUData(icuDataResource: LoadingResource): Promise<void> {
  const runDependencyId = `wamsdk:icudata`;
  addRunDependency(runDependencyId);

  const request = await icuDataResource.response;
  const array = new Uint8Array(await request.arrayBuffer());

  const offset = MONO.mono_wasm_load_bytes_into_heap(array);
  if (!MONO.mono_wasm_load_icu_data(offset)) {
    throw new Error("Error loading ICU asset.");
  }
  removeRunDependency(runDependencyId);
}

async function compileWasmModule(
  wasmResource: LoadingResource,
  imports: any
): Promise<WebAssembly.Instance> {
  // This is the same logic as used in emscripten's generated js. We can't use emscripten's js because
  // it doesn't provide any method for supplying a custom response provider, and we want to integrate
  // with our resource loader cache.

  if (typeof WebAssembly["instantiateStreaming"] === "function") {
    try {
      const streamingResult = await WebAssembly["instantiateStreaming"](
        wasmResource.response,
        imports
      );
      return streamingResult.instance;
    } catch (ex) {
      console.info(
        "Streaming compilation failed. Falling back to ArrayBuffer instantiation. ",
        ex
      );
    }
  }

  // If that's not available or fails (e.g., due to incorrect content-type header),
  // fall back to ArrayBuffer instantiation
  const arrayBuffer = await wasmResource.response.then((r) => r.arrayBuffer());
  const arrayBufferResult = await WebAssembly.instantiate(arrayBuffer, imports);
  return arrayBufferResult.instance;
}

function changeExtension(filename: string, newExtensionWithLeadingDot: string) {
  const lastDotIndex = filename.lastIndexOf(".");
  if (lastDotIndex < 0) {
    throw new Error(`No extension to replace in '${filename}'`);
  }

  return filename.substr(0, lastDotIndex) + newExtensionWithLeadingDot;
}

function assertHeapIsNotLocked() {
  if (currentHeapLock) {
    throw new Error("Assertion failed - heap is currently locked");
  }
}

class MonoHeapLock implements HeapLock {
  // Within a given heap lock, it's safe to cache decoded strings since the memory can't change
  stringCache = new Map<number, string | null>();

  private postReleaseActions?: Function[];

  enqueuePostReleaseAction(callback: Function) {
    if (!this.postReleaseActions) {
      this.postReleaseActions = [];
    }

    this.postReleaseActions.push(callback);
  }

  release() {
    if (currentHeapLock !== this) {
      throw new Error("Trying to release a lock which isn't current");
    }

    currentHeapLock = null;

    while (this.postReleaseActions?.length) {
      const nextQueuedAction = this.postReleaseActions.shift()!;

      // It's possible that the action we invoke here might itself take a succession of heap locks,
      // but since heap locks must be released synchronously, by the time we get back to this stack
      // frame, we know the heap should no longer be locked.
      nextQueuedAction();
      assertHeapIsNotLocked();
    }
  }
}
