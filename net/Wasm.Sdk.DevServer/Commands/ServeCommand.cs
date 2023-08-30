// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Wasm.Sdk.DevServer.Commands
{
    internal class ServeCommand : CommandLineApplication
    {
        public ServeCommand(CommandLineApplication parent)

            // We pass arbitrary arguments through to the ASP.NET Core configuration
            : base(throwOnUnexpectedArg: false)
        {
            Parent = parent;

            Name = "serve";
            Description = "Serve requests to a Wasm application";

            HelpOption("-?|-h|--help");

            OnExecute(Execute);
        }

        private int Execute()
        {
            var webHost = BuildWebHost(RemainingArguments.ToArray());
            webHost.Run();

            return 0;
        }

        public static IHost BuildWebHost(string[] args) => DevServerHost.CreateHost(args);

    }

    public static class DevServerHost
    {
        public static IHost CreateHost(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(config =>
                {
                    var applicationPath = args.SkipWhile(a => a != "--applicationpath").Skip(1).FirstOrDefault();
                    var applicationDirectory = Path.GetDirectoryName(applicationPath);
                    var name = Path.ChangeExtension(applicationPath, ".StaticWebAssets.xml");

                    var inMemoryConfiguration = new Dictionary<string, string>
                    {
                        [WebHostDefaults.EnvironmentKey] = "Development",
                        ["Logging:LogLevel:Microsoft"] = "Warning",
                        ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information",
                        [WebHostDefaults.StaticWebAssetsKey] = name,
                    };

                    config.AddInMemoryCollection(inMemoryConfiguration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStaticWebAssets();
                    webBuilder.UseStartup<Startup>();
                }).Build();
    }
}
