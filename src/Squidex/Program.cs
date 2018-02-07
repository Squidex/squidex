// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Squidex.Config.Orleans;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var silo = new SiloHostBuilder()
                .UseConfiguration(ClusterConfiguration.LocalhostPrimarySilo(33333))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices((context, services) =>
                {
                    services.AddAppSiloServices(context.Configuration);
                    services.AddAppServices(context.Configuration);
                })
                .ConfigureApplicationParts(builder =>
                {
                    builder.AddApplicationPart(typeof(EventConsumerManagerGrain).Assembly);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddSemanticLog();
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.AddAppConfiguration(GetEnvironment(), args);
                })
                .Build();

            silo.StartAsync().Wait();

            try
            {
                new WebHostBuilder()
                    .UseKestrel(k => { k.AddServerHeader = false; })
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<WebStartup>()
                    .ConfigureLogging(builder =>
                    {
                        builder.AddSemanticLog();
                    })
                    .ConfigureAppConfiguration((hostContext, builder) =>
                    {
                        builder.AddAppConfiguration(hostContext.HostingEnvironment.EnvironmentName, args);
                    })
                    .Build()
                    .Run();
            }
            finally
            {
                silo.StopAsync().Wait();
            }
        }

        private static string GetEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return environment ?? "Development";
        }
    }
}
