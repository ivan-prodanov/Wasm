using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;

namespace Wasm.Sdk.Tasks
{
    public partial class PublishDocumentationTask : Task
    {
        [Required]
        public string AssemblyPatterns { get; set; } = "";

        [Required]
        public string DefaultAssemblyPatterns { get; set; } = "";

        [Required]
        public string SourceDirectory { get; set; } = "";

        [Required]
        public string OutputDirectory { get; set; } = "";

        public override bool Execute()
        {
            try
            {
                var defaultPatterns = DefaultAssemblyPatterns.Split(';');
                foreach (var asmPattern in defaultPatterns)
                {
                    var docs = Directory.EnumerateFiles(SourceDirectory, $"{asmPattern}"); // Do not recurse search!
                    foreach (var doc in docs)
                    {
                        var destinationDoc = Path.Combine(OutputDirectory, Path.GetFileName(doc));
                        File.Copy(doc, destinationDoc, true);
                    }
                }

                var patterns = AssemblyPatterns.Split(';');
                foreach (var asmPattern in patterns)
                {
                    var docs = Directory.EnumerateFiles(SourceDirectory, $"{asmPattern}"); // Do not recurse search!
                    foreach (var doc in docs)
                    {
                        var destinationDoc = Path.Combine(OutputDirectory, Path.GetFileName(doc));
                        File.Copy(doc, destinationDoc, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString(), false, true, null);
            }

            return !Log.HasLoggedErrors;
        }
    }
}
