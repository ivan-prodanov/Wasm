namespace Wasm.Sdk.Analyzer.Generator
{
    public interface IGeneratorOptions
    {
        bool EnableDebug { get; set; }
        string IntermediateOutputPath { get; set; }
        string ProjectRootPath { get; set; }
    }
}