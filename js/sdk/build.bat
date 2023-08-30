set srcpath=%cd%
cd "..\.."
powershell .\scripts\net-wasm-build.ps1 -outputDirectory "%srcpath%%\dist\_framework"