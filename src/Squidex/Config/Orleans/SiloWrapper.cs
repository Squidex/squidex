// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log.Adapter;

namespace Squidex.Config.Orleans
{
    public class SiloWrapper : IInitializable, IDisposable
    {
        private readonly ISiloHost silo;

        internal sealed class Source : IConfigurationSource
        {
            private readonly IConfigurationProvider configurationProvider;

            public Source(IConfigurationProvider configurationProvider)
            {
                this.configurationProvider = configurationProvider;
            }

            public IConfigurationProvider Build(IConfigurationBuilder builder)
            {
                return configurationProvider;
            }
        }

        public SiloWrapper(IConfiguration configuration)
        {
            silo = SiloHostBuilder.CreateDefault()
               .UseConfiguration(ClusterConfiguration.LocalhostPrimarySilo(33333))
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseDashboard(options =>
               {
                   options.HostSelf = false;
               })
               .ConfigureApplicationParts(builder =>
               {
                   builder.AddApplicationPart(SquidexInfrastructure.Assembly);
               })
               .ConfigureLogging(builder =>
               {
                   builder.AddSemanticLog();
               })
               .ConfigureServices((context, services) =>
               {
                   services.AddAppSiloServices(context.Configuration);
                   services.AddAppServices(context.Configuration);
               })
               .ConfigureAppConfiguration((hostContext, builder) =>
               {
                   if (configuration is IConfigurationRoot root)
                   {
                       foreach (var provider in root.Providers)
                       {
                           builder.Add(new Source(provider));
                       }
                   }
               })
               .Build();
        }

        public void Initialize()
        {
            silo.StartAsync().Wait();
        }

        public void Dispose()
        {
            silo.StopAsync().Wait();
        }

        private static string GetEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return environment ?? "Development";
        }
    }
}
