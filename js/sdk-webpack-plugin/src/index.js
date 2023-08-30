import CopyPlugin from "copy-webpack-plugin";
import path from "path";
import fs, { readdirSync } from "fs";
import { uniq, uniqBy, groupBy } from "lodash";
import glob from "glob";

const pluginName = "WasmSdkPlugin";

export default class WasmSdkPlugin {
  constructor(
    {
      root = "dist",
      moduleDirectories = ["node_modules"],
      whitelistedFrameworkDirs = ["build", "dist"],
      destination,
      ignoreMultipleRuntimes = false,
      ignoreAssemblyCollision = false,
    } = {
      root: "dist",
      moduleDirectories: ["node_modules"],
      whitelistedFrameworkDirs: ["build", "dist"],
      ignoreMultipleRuntimes: false,
      ignoreAssemblyCollision: false,
    }
  ) {
    this._wasmPackageFile = "wasm.package.json";
    this._wasmRuntimeFile = "wasm.runtime.json";

    this.moduleDirectories = moduleDirectories;
    this.whitelistedFrameworkDirs = whitelistedFrameworkDirs;
    this.destination = destination;
    this.ignoreMultipleRuntimes = ignoreMultipleRuntimes;
    this.ignoreAssemblyCollision = ignoreAssemblyCollision;
    this.runtime = null;
    const symlinkDirectories = this.moduleDirectories
      .flatMap(x =>  {
        let directories = readdirSync(x, { withFileTypes: true });
        let output = [];

        for (const dir of directories){
          if(dir.isDirectory() === false && dir.isSymbolicLink() === false) {
            continue;
          }

          const searchPath = path.resolve(x, dir.name);
          if(dir.isSymbolicLink()) {
            output.push(fs.readlinkSync(searchPath));
          } else if(dir.name.startsWith('@')) {
            const childDirs = readdirSync(searchPath, { withFileTypes: true })
              .filter(cd => cd.isSymbolicLink())
              .map(cd => fs.readlinkSync(path.resolve(searchPath, cd.name)));
             
            output = output.concat(childDirs);
          }
        }

        return output;
      });

    const packageDirectories = [
      ...new Set(
        this.moduleDirectories
        .concat(symlinkDirectories)
          .map((directory) =>
            path.resolve(root, `${directory}/**/_framework`).replace(/\\/g, "/")
          )
          .flatMap((p) => {
            const target = path.resolve(
              p,
              `?(${this._wasmPackageFile}|${this._wasmRuntimeFile})`
            );

            var packages = glob.sync(target);

            var runtimePackages = packages.filter(
              (p) => p.indexOf(this._wasmRuntimeFile) !== -1
            );

            if (runtimePackages.length > 1) {
              this.ensureWasmRuntime();
            }

            packages = packages.filter(
              (p) => runtimePackages.indexOf(p) === -1 && 
              this.whitelistedFrameworkDirs.some(d => p.indexOf(d) !== -1)
            );

            if (runtimePackages.length !== 0) {
              // there should be a single source of the runtime
              const runtimeDirectory = path.dirname(runtimePackages[0]);

              if (this.runtime && this.runtime.directory !== runtimeDirectory) {
                console.warn(
                  `Runtimes at: [${this.runtime.directory}, ${runtimeDirectory}]`
                );
                this.ensureWasmRuntime();
              }

              this.runtime = {
                content: JSON.parse(fs.readFileSync(runtimePackages[0])),
                directory: runtimeDirectory,
              };
            }

            return packages;
          })
      ),
    ];

    this.packages = packageDirectories.map((p) => ({
      content: JSON.parse(fs.readFileSync(p)),
      directory: path.dirname(p),
    }));

    this.copyPlugin = new CopyPlugin({
      // copy all the packages
      patterns: [
        ...this.packages.map(({ directory, content }) => ({
          from: path.posix.join(
            directory,
            "*.(blat|wasm|dat|dll|pdb|xml|json)"
          ),
          to: `${destination}\\${content.id}\\[name].[ext]`,
        })),
        // copy the runtime dlls
        {
          from: path.posix.join(
            this.runtime.directory,
            "*.(blat|wasm|dat|dll|pdb|xml)"
          ),
          to: `${destination}\\runtime\\[name].[ext]`,
        },
        // copy and hash js files
        {
          from: path.posix.join(this.runtime.directory, "*.js"),
          to: `${destination}\\runtime\\[name].[hash].[ext]`,
        },
      ],
    });
  }

  ensureWasmRuntime() {
    if (!this.ignoreMultipleRuntimes) {
      throw new Error(
        "Multiple runtimes of 0x33/wasm-sdk detected. Make sure to have a single source or set the ignoreMultipleRuntimes to true"
      );
    } else {
      console.warn(
        "Multiple runtimes of 0x33/wasm-sdk detected. Using LI priority to resolve."
      );
    }
  }

  isolateUniqueAssemblies(assemblies) {
    return uniqBy(assemblies, (a) => a.name);
  }

  hasPackageCollisions(assemblies) {
    const collisionDictionary = groupBy(assemblies, (a) => a.name);
    return Object.keys(collisionDictionary)
      .map((assemblyName) => {
        const targetAssemblies = collisionDictionary[assemblyName];

        if (targetAssemblies.length <= 1) {
          return false;
        }

        const uniqueAssemblies = uniqBy(targetAssemblies, (x) =>
          [x.version, x.size].join()
        );

        if (uniqueAssemblies.length != 1) {
          console.error(`Detected collision for assembly: ${assemblyName}`);
          return true;
        }

        return false;
      })
      .some((x) => x);
  }

  async apply(compiler) {
    await this.copyPlugin.apply(compiler);

    compiler.hooks.emit.tapAsync(pluginName, async (compilation, callback) => {
      console.log(this.packages);

      const assemblies = this.packages.flatMap((p) => p.content.assemblies);

      const hasPackageCollisions = this.hasPackageCollisions(assemblies);

      if (hasPackageCollisions) {
        if (this.ignoreAssemblyCollision) {
          console.warn(
            "Package collisions detected [Using soft-collision-rules (version, size)]. Allowing execution with warning"
          );
        } else {
          throw new Error(
            "Package collision detected [Using soft-collision-rules (version, size)]. Resolve the conflicts or set the ignoreAssemblyCollision flag to true."
          );
        }
      }

      const dotnetRuntimeAsset = compilation
        .getAssets()
        .find(
          ({ name }) =>
            path.extname(name) === ".js" &&
            path.basename(name).indexOf("dotnet") !== -1
        );

      if (!dotnetRuntimeAsset) {
        throw new Error("Missing dotnet.js runtime.");
      }

      const appJson = {
        ...this.runtime.content,
        js: {
          ...this.runtime.content.js,
          name: path.basename(dotnetRuntimeAsset.name),
        },
        linkerEnabled: false,
        //TODO need to wire linkerEnabled up
        cacheBootResources: false,
        //TODO need to wire cacheBootResources up
        packages: this.packages.map((p) => p.content.id),
      };
      
      compilation.assets[`${this.destination}\\wasm.app.json`] = {
        source: function () {
          return JSON.stringify(appJson);
        },
        size: function () {
          return appJson.packages.length;
        },
      };
      callback();
    });
  }
}
