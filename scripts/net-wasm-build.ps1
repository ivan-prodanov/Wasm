param($outputDirectory)

# Set-Location -Path '..' -PassThru

dotnet build '.\net\Wasm.Sdk' --output '.\build'

Remove-Item $outputDirectory -Recurse -ErrorAction Ignore
Copy-Item -Path '.\build\package' -Destination $outputDirectory -Recurse -Force 

Remove-Item '.\build' -Recurse