using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Wasm.Sdk.Tasks
{
    [DataContract]
    public class NpmPackage
    {
        public NpmPackage(string name, string version, string author, string description, string license)
        {
            Name = name;
            Version = version;
            Author = author;
            Description = description;
            License = license;

            Scripts = new Scripts();
            DevDependencies = new Dictionary<string, string>()
            {
                {"rimraf", "2.7.1"},
                {"ts-loader", "8.0.11"},
                {"typescript", "5.1.6"},
                {"webpack", "5.10.0"},
                {"webpack-cli", "4.2.0"},
                {"copy-webpack-plugin", "6.4.0"}
            };
        }

        [DataMember(Order = 0, Name = "name")]
        public string Name { get; set; }
        [DataMember(Order = 1, Name = "version")]
        public string Version { get; set; }
        [DataMember(Order = 2, Name = "author")]
        public string Author { get; set; }
        [DataMember(Order = 3, Name = "description")]
        public string Description { get; set; }
        [DataMember(Order = 4, Name = "license")]
        public string License { get; set; }

        [DataMember(Order = 5, Name = "main")]
        public string Main { get; set; } = "dist/index.js";
        [DataMember(Order = 6, Name = "types")]
        public string Types { get; set; } = "dist/index.d.ts";

        [DataMember(Order = 7, Name = "wasm")]
        public bool Wasm { get; set; } = true;

        [DataMember(Order = 8, Name = "scripts")]
        public Scripts Scripts { get; set; }

        [DataMember(Order = 9, Name = "devDependencies")]
        public Dictionary<string, string> DevDependencies { get; set; }

        [DataMember(Order = 10, Name = "files")]
        public List<string> Files { get; set; } = new List<string> { "dist" };
    }

    [DataContract]
    public class Scripts
    {
        public Scripts()
        {
            Clean = "rimraf dist";
            Build = "node node_modules/webpack-cli/bin/cli.js --mode production --config ./webpack.config.js";
            BuildDebug = "node node_modules/webpack-cli/bin/cli.js --mode development --config ./webpack.config.js";
        }

        [DataMember(Order = 0, Name = "clean")]
        public string Clean { get; set; }

        [DataMember(Order = 1, Name = "build")]
        public string Build { get; set; }

        [DataMember(Order = 1, Name = "build:debug")]
        public string BuildDebug { get; set; }
    }
}
