// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Squidex.Config;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            string envName = Environment.GetEnvironmentVariable("Environment");
            string envConfigFile = $"./conf/appsettings.{envName}.json";
            string envSecretsFile = $"./secrets/appsettings.secrets.json";

            return new WebHostBuilder()
                .UseKestrel(k => { k.AddServerHeader = false; })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIIS()
                .UseStartup<WebStartup>()
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("logging"));
                    builder.AddSemanticLog();
                    builder.AddFilters();
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.Sources.Clear();

                    builder.AddJsonFile($"appsettings.json", true);
                    builder.AddJsonFile($"appsettings.Custom.json", true);
                    builder.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);
                    builder.AddJsonFile(envSecretsFile, true, true);
                    builder.AddJsonFile(envConfigFile, true, true);

                    builder.AddEnvironmentVariables();

                    builder.AddCommandLine(args);
                })
                .Build();
        }
    }
}
