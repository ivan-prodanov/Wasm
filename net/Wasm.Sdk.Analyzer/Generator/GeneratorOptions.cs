using Microsoft.CodeAnalysis;
using System;

namespace Wasm.Sdk.Analyzer.Generator
{
    internal class GeneratorOptions : IGeneratorOptions
    {
        public GeneratorOptions(GeneratorExecutionContext context)
        {
            var intermediateOutputVariableName = "IntermediateOutputPath";
            if (TryReadGlobalOption(context, intermediateOutputVariableName, out var intermediate))
            {
                IntermediateOutputPath = intermediate;
            }
            else
            {
                throw new NotSupportedException($"The variable {intermediateOutputVariableName} is not visible in the source generator.");
            }

            var projectDirVariableName = "ProjectDir";
            if (TryReadGlobalOption(context, projectDirVariableName, out var projectRoot))
            {
                ProjectRootPath = projectRoot;
            }
            else
            {
                throw new NotSupportedException($"The variable {projectDirVariableName} is not visible in the source generator.");
            }

            if (TryReadGlobalOption(context, "SourceGenerator_EnableDebug", out var enableDebug) && bool.TryParse(enableDebug, out var enableDebugValue))
            {
                EnableDebug = enableDebugValue;
            }
        }

        public bool EnableDebug { get; set; }
        public string IntermediateOutputPath { get; set; }
        public string ProjectRootPath { get; set; }

        public bool TryReadGlobalOption(GeneratorExecutionContext context, string property, out string value)
        {
            return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{property}", out value);
        }

        public bool TryReadAdditionalFilesOption(GeneratorExecutionContext context, AdditionalText additionalText,
            string property, out string value)
        {
            return context.AnalyzerConfigOptions.GetOptions(additionalText)
                .TryGetValue($"build_metadata.AdditionalFiles.{property}", out value);
        }
    }
}