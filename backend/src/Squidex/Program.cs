// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Config.Domain;
using Squidex.Config.Startup;

namespace Squidex;

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
            .ConfigureServices((context, services) =>
            {
                // Step 0: Log all configuration.
                services.AddHostedService<LogConfigurationHost>();

                // Step 1: Initialize all services.
                services.AddInitializer();

                // Step 2: Run migration.
                services.AddHostedService<MigratorHost>();

                // Step 3: Run rebuild processes.
                services.AddHostedService<MigrationRebuilderHost>();

                // Step 4: Start background processes.
                services.AddBackgroundProcesses();
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.ConfigureKestrel((context, serverOptions) =>
                {
                    if (context.HostingEnvironment.IsDevelopment() || context.Configuration.GetValue<bool>("devMode:enable"))
                    {
                        serverOptions.ListenAnyIP(
                            5001,
                            listenOptions => listenOptions.UseHttps("../../../dev/squidex-dev.pfx", "password"));

                        serverOptions.ListenAnyIP(5000);
                    }
                });

                builder.UseStartup<Startup>();
            });
}
