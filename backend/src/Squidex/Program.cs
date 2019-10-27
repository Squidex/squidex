﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Config.Domain;
using Squidex.Config.Orleans;
using Squidex.Config.Startup;

namespace Squidex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, builder) =>
                {
                    builder.ConfigureForSquidex(context.Configuration);
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.ConfigureForSquidex();
                })
                .ConfigureServices(services =>
                {
                    // Step 0: Log all configuration.
                    services.AddHostedService<LogConfigurationHost>();

                    // Step 1: Initialize all services.
                    services.AddHostedService<InitializerHost>();

                    // Step 2: Create admin user.
                    services.AddHostedService<CreateAdminHost>();
                })
                .UseOrleans((context, builder) =>
                {
                    // Step 3: Start Orleans.
                    builder.ConfigureForSquidex(context.Configuration);
                })
                .ConfigureServices(services =>
                {
                    // Step 4: Run migration.
                    services.AddHostedService<MigratorHost>();

                    // Step 5: Run rebuild processes.
                    services.AddHostedService<MigrationRebuilderHost>();

                    // Step 6: Start background processes.
                    services.AddHostedService<BackgroundHost>();
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                });
    }
}
