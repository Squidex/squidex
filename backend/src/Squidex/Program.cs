// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Config.Domain;
using Squidex.Config.Orleans;

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
}
