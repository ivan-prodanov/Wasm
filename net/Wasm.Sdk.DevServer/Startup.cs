// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Wasm.Sdk.DevServer
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseDeveloperExceptionPage();
            EnableConfiguredPathbase(app, configuration);

            app.UseWebAssemblyDebugging();

            app.UseStaticFiles(new StaticFileOptions
            {
                // In development, serve everything, as there's no other way to configure it.
                // In production, developers are responsible for configuring their own production server
                ServeUnknownFileTypes = true,
            });

            app.UseRouting();

            if (configuration["redirect"] is string redirectUri && !string.IsNullOrEmpty(redirectUri))
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", context =>
                    {
                        context.Response.Redirect(redirectUri);
                        return Task.CompletedTask;
                    });
                });
            }
            else if (configuration["fallbackpath"] is string fallbackPath && !string.IsNullOrEmpty(fallbackPath))
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapFallbackToFile(fallbackPath);
                });
            }
            else
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        var response = "<div style='font-family: Consolas,monaco,monospace'>" +
                                           "<h3>" +
                                           "Debugger attached! Set a breakpoint and browse your app's url" +
                                           "</h3>" +
                                           "<div>" +
                                           "Automatic redirects can be configured using the following property in the .csproj file:" +
                                           "</div>" +
                                           "<div>" +
                                           "<pre style='padding: 16px; border: 1px dotted rgb(162 162 162)'>" +
                                           "    <b>&lt;PropertyGroup&gt;</b>" +
                                           "    <br />" +
                                           "    &nbsp;<b>&lt;Redirect&gt;</b>http://LinkToYourApp<b>&lt;/Redirect&gt;</b>" +
                                           "    <br />" +
                                           "    <b>&lt;/PropertyGroup&gt;</b>" +
                                           "</pre>" +
                                           "</div>" +
                                           "<div>" +
                                           "Alternatively a file fallback can be configured like so:" +
                                           "</div>" +
                                           "<div>" +
                                           "<pre style='padding: 16px; border: 1px dotted rgb(162 162 162)'>" +
                                           "    <b>&lt;PropertyGroup&gt;</b>" +
                                           "    <br />" +
                                           "    &nbsp;&lt;<b>FallbackPath&gt;</b>index.html<b>&lt;/FallbackPath&gt;</b>" +
                                           "    <br />" +
                                           "    <b>&lt;/PropertyGroup&gt;</b>" +
                                           "</pre>" +
                                           "</div>" +
                                       "</div>";

                        context.Response.Headers.Add("Cache-Control", "no-cache");
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(response, Encoding.UTF8);
                    });
                });
            }
        }

        private static void EnableConfiguredPathbase(IApplicationBuilder app, IConfiguration configuration)
        {
            var pathBase = configuration.GetValue<string>("pathbase");
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);

                // To ensure consistency with a production environment, only handle requests
                // that match the specified pathbase.
                app.Use((context, next) =>
                {
                    if (context.Request.PathBase == pathBase)
                    {
                        return next();
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        return context.Response.WriteAsync($"The server is configured only to " +
                            $"handle request URIs within the PathBase '{pathBase}'.");
                    }
                });
            }
        }
    }
}
