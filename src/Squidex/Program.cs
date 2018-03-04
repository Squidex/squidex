// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel(k => { k.AddServerHeader = false; })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<WebStartup>()
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("logging"));
                    builder.AddSemanticLog();
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.Sources.Clear();

                    builder.AddJsonFile("appsettings.json", true, true);
                    builder.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);

                    builder.AddEnvironmentVariables();

                    builder.AddCommandLine(args);
                })
                .Build()
                .Run();
        }
    }
}
