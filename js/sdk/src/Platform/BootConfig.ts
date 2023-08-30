export class BootConfigResult {
  private constructor(
    public bootConfig: BootJsonData,
    public applicationEnvironment: string
  ) {}

  static async initAsync({
    environment = "Production",
    configResolver,
    targetPackages = [],
  }: {
    environment?: string;
    configResolver: (configName: string) => string;
    targetPackages?: string[];
  }): Promise<BootConfigResult> {
    const bootConfigResponse = await fetch(configResolver("wasm.app.json"), {
      method: "GET",
      credentials: "include",
      cache: "no-cache",
    });

    // While we can expect an ASP.NET Core hosted application to include the environment, other
    // hosts may not. Assume 'Production' in the absence of any specified value.
    const plainBootConfig: PlainBootJsonData = await bootConfigResponse.json();

    const packages = await Promise.all(
      plainBootConfig.packages
        .filter((p) => targetPackages.indexOf(p) !== -1)
        .map(async (p) => {
          const packageResponse = await fetch(
            configResolver(`${p}\\wasm.package.json`),
            {
              method: "GET",
              credentials: "include",
              cache: "no-cache",
            }
          );

          return await packageResponse.json();
        })
    );

    const bootConfig: BootJsonData = {
      ...plainBootConfig,
      packages,
    };

    return new BootConfigResult(bootConfig, environment);
  }
}

// Keep in sync with bootJsonData in Microsoft.AspNetCore.Components.WebAssembly.Build
export interface PlainBootJsonData {
  readonly debugBuild: boolean;
  readonly icuDataMode: ICUDataMode;

  readonly linkerEnabled: boolean;
  readonly cacheBootResources: boolean;

  readonly wasm: WasmFile;
  readonly js: JsFile;
  readonly icu: IcuFile[];
  readonly tz: TimeZoneFile;

  readonly packages: string[];
}

export interface BootJsonData extends Omit<PlainBootJsonData, "packages"> {
  readonly packages: BootPackage[];
}

export interface BootPackage {
  readonly id: string;
  readonly name: string;
  readonly assemblies: AssemblyFile[];
}

export interface ContentFile {
  readonly name: string;
  readonly size: number;
  readonly hash: string;
}

export interface PdbFile extends ContentFile {}
export interface DocFile extends ContentFile {}
export interface WasmFile extends ContentFile {}
export interface TimeZoneFile extends ContentFile {}
export interface JsFile extends ContentFile {}
export interface IcuFile extends ContentFile {}

export interface AssemblyFile extends ContentFile {
  readonly version: string;
  readonly pdb?: PdbFile;
  readonly doc?: DocFile;
}

export enum ICUDataMode {
  Sharded,
  All,
  Invariant,
}
