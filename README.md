Framework for packaging existing C# code into WASM-enabled packages that ready to be loaded into existing react/angular/etc apps.

Represents the following NuGet packages:
1. https://www.nuget.org/packages/Wasm.Sdk/
2. https://www.nuget.org/packages/Wasm.Sdk.DevServer/
3. https://www.nuget.org/packages/Wasm.Sdk.Analyzer/
4. https://www.nuget.org/packages/Wasm.Sdk.Mono/


To get started, in VS create a new Library project and modify the .csproj as so:

```xml
<Project Sdk="Wasm.Sdk/1.0.0">
	<PropertyGroup>
		<TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
</Project
```

This alone installs the NuGet packages and hooks the project to use the Wasm.SDK.


# Controllers

To create endpoints accessible from the browser, create a class and wrap it using the [Wasm] attribute.

```csharp

    [Wasm]
    public class WasmRunner
    {
        private readonly string baseUri;

        public WasmRunner(string baseUri)
        {
            this.baseUri = baseUri;
        }
        public void ExecuteAssembly(byte[] assembly)
        {
            var weakRef = WasmExecutor.ExecuteAndUnload(assembly, asm =>
            {
                var type = asm.GetType("ConsoleApp1.Program");
                var methodInfo = type.GetMethod("Main", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var method = methodInfo.CreateDelegate<Action<string[]>>();
                method(new string[] { "You love easter eggs, don't you? There are two WASM instances running", "One for the C# compiler", "And one for execution", "Each instance is running in its own web worker (thread)" });
            });
        }
    }

```

# Examples:
1. WasmRunner: https://github.com/ivan-prodanov/WasmRunner
2. OmniWasm: https://github.com/ivan-prodanov/OmniWasm
3. Consuming WASM.Packages: https://github.com/ivan-prodanov/CSharpWasmCompiler

Demo project: https://0x33.io 
