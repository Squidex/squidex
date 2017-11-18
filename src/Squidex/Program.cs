// ==========================================================================
//  Program.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Squidex.Config.Orleans;
using Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations;
using Squidex.Domain.Users.DataProtection.Orleans.Grains.Implementations;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var silo = new SiloHostBuilder()
                .AddApplicationPartsFromReferences(typeof(AppStateGrain).Assembly)
                .AddApplicationPartsFromReferences(typeof(EventConsumerGrain).Assembly)
                .AddApplicationPartsFromReferences(typeof(XmlRepositoryGrain).Assembly)
                .UseDashboard(options => { options.HostSelf = false; })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(
                    ClusterConfiguration.LocalhostPrimarySilo(33333)
                        .WithJsonSerializer()
                        .WithDashboard())
                .ConfigureServices((context, services) =>
                {
                    services.AddAppSiloServices(context.Configuration);
                    services.AddAppServices(context.Configuration);
                })
                .ConfigureLogging(builder =>
                {
                    // builder.AddSemanticLog();
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
