import * as Environment from "./Environment";
import "./GlobalExports";
import { BootConfigResult, BootJsonData } from "./Platform/BootConfig";
import { monoPlatform } from "./Platform/Mono/MonoPlatform";
import { WebAssemblyResourceLoader } from "./Platform/WebAssemblyResourceLoader";
import { WebAssemblyStartOptions } from "./Platform/WebAssemblyStartOptions";

let started = false;

export async function boot(
  options: Partial<WebAssemblyStartOptions> = {
    environment: "Production",
    pathResolver: (path) => `_framework\\${path}`,
    onConsoleLog: (line) => console.log(line),
    onConsoleErr: (line) => console.error(line),
    onRuntimeErr: (line) => console.error(line),
  }
): Promise<BootJsonData> {
  if (started) {
    console.warn("WasmSdk has already been initialized in this thread.");
  }
  started = true;

  const platform = Environment.setPlatform(monoPlatform);

  // Configure environment for execution under Mono WebAssembly with shared-memory rendering
  // Get the custom environment setting if defined
  const environment = options.environment || "Production";
  const pathResolver = options.pathResolver || ((p) => `_framework\\${p}`);
  const consoleLog = options.onConsoleLog || ((l) => console.log(l));
  const consoleErr = options.onConsoleErr || ((l) => console.error(l));
  const runtimeErr = options.onRuntimeErr || ((l) => console.error(l));

  // Fetch the resources and prepare the Mono runtime
  const bootConfigResult = await BootConfigResult.initAsync({
    environment,
    configResolver: pathResolver,
    targetPackages: options?.packages,
  });

  let resourceLoader = await WebAssemblyResourceLoader.initAsync(
    bootConfigResult.bootConfig,
    options || {}
  );

  try {
    await platform.start(
      resourceLoader,
      pathResolver,
      consoleLog,
      consoleErr,
      runtimeErr
    );

    return bootConfigResult.bootConfig;
  } catch (ex) {
    throw new Error(`Failed to start platform. Reason: ${ex}`);
  }
}
