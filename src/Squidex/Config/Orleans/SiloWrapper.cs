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
using Squidex.Config.Domain;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log.Adapter;
using Squidex.Infrastructure.Orleans;

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
            J.Serializer = SerializationServices.DefaultJsonSerializer;

            silo = SiloHostBuilder.CreateDefault()
               .UseConfiguration(ClusterConfiguration.LocalhostPrimarySilo(33333).WithDashboard())
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseDashboard(options =>
               {
                   options.HostSelf = false;
               })
               .ConfigureLogging(builder =>
               {
                   builder.AddSemanticLog();
               })
               .ConfigureApplicationParts(builder =>
               {
                   builder.AddApplicationPart(SquidexEntities.Assembly);
                   builder.AddApplicationPart(SquidexInfrastructure.Assembly);
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
    }
}
