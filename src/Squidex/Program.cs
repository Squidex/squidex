// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Squidex.Config;
using Squidex.Config.Orleans;
using Squidex.Config.Startup;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseDefaultServiceProvider(options =>
                {
                    options.ValidateOnBuild = false;
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("logging"));
                    builder.AddSemanticLog();
                    builder.AddFilters();
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);

                    builder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.Sources.Clear();

                    builder.AddJsonFile($"appsettings.json", true);
                    builder.AddJsonFile($"appsettings.Custom.json", true);
                    builder.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);

                    builder.AddEnvironmentVariables();

                    builder.AddCommandLine(args);
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<InitializerHost>();
                })
                .UseOrleans((context, builder) =>
                {
                    builder.ConfigureOrleans(context.Configuration);
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<MigratorHost>();
                    services.AddHostedService<MigrationRebuilderHost>();
                    services.AddHostedService<BackgroundHost>();
                });
    }
}
