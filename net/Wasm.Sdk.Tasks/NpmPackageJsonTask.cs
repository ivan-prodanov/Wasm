using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Wasm.Sdk.Tasks
{
    public class NpmPackageJsonTask : Task
	{
        private const string PackageFileName = "package.json";

        [Required]
        public string? Name { get; set; }
        public string? PackageName { get; set; }
        [Required]
        public string? Version { get; set; }
        public string? Authors { get; set; }
        public string? Description { get; set; }
        public string? License { get; set; }

        [Required]
        public string? OutputPath { get; set; }

        public override bool Execute()
        {
            var packageFileName = Path.Combine(OutputPath, PackageFileName);

            try
            {
                if (File.Exists(packageFileName))
                {
                    var json = File.ReadAllText(packageFileName);
                    var output = UpdatePackageJson(json);
                    File.WriteAllText(packageFileName, output);
                }
                else
                {
                    using var fileStream = File.Create(packageFileName);
                    WritePackageJson(fileStream);
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }

        private void ValidateStringField(string? value, string valueName)
        {
            if (value == default)
            {
                throw new ArgumentNullException($"{valueName} is not provided");
            }
        }

        public static string? ConvertToPackageName(string? source)
        {
            if (source is null) return null;

            if (source.Length == 0) return string.Empty;

            // Remove invalid characters
            var packageName = Regex.Replace(source, "[~\"'!()*]", string.Empty);

            // Replace spaces and dots with dashes
            packageName = Regex.Replace(packageName, "[\\s\\.]", "-");

            return packageName.ToLower();
        }

        public void WritePackageJson(Stream output)
        {
            var packageName = PackageName ?? ConvertToPackageName(Name);
            var description = Description ?? $"Auto-generated .net wasm package that exports ts/js consumables for {Name}";

            ValidateStringField(packageName, nameof(Name));
            ValidateStringField(Version, nameof(Version));
            ValidateStringField(Authors, nameof(Authors));
            //ValidateStringField(Description, nameof(Description));

            License = string.IsNullOrWhiteSpace(License) ? "MIT" : License;

#pragma warning disable CS8604 // Possible null reference argument.
            var bootConfiguration = new NpmPackage(packageName, Version, Authors, description, License);
#pragma warning restore CS8604 // Possible null reference argument.

            var serializer = new DataContractJsonSerializer(typeof(NpmPackage), new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            });

            using var writer = JsonReaderWriterFactory.CreateJsonWriter(output, Encoding.UTF8, ownsStream: false, indent: true);
            serializer.WriteObject(writer, bootConfiguration);
        }

        public string UpdatePackageJson(string content)
        {
            var packageName = PackageName ?? ConvertToPackageName(Name);
            ValidateStringField(packageName, nameof(Name));
            ValidateStringField(Version, nameof(Version));

            using var deserializedPackage = JsonDocument.Parse(content);
            if (deserializedPackage == null)
            {
                return content;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions 
            { 
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            writer.WriteStartObject();

            foreach (var el in deserializedPackage.RootElement.EnumerateObject())
            {
                switch (el.Name)
                {
                    case "name":
                        {
                            writer.WritePropertyName(el.Name);
                            writer.WriteStringValue(packageName);
                            break;
                        }
                    case "version":
                        {
                            writer.WritePropertyName(el.Name);
                            writer.WriteStringValue(Version);
                            break;
                        }
                    default:
                        {
                            el.WriteTo(writer);
                            break;
                        }
                }
            }

            writer.WriteEndObject();
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
