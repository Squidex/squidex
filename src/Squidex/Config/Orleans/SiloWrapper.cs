// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Squidex.Config.Domain;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Log.Adapter;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Config.Orleans
{
    public class SiloWrapper : DisposableObjectBase, IInitializable, IDisposable
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

            silo = new SiloHostBuilder()
                .UseDashboard(options => options.HostSelf = true)
                .AddStartupTask<Bootstrap<IContentSchedulerGrain>>()
                .AddStartupTask<Bootstrap<IEventConsumerManagerGrain>>()
                .AddStartupTask<Bootstrap<IRuleDequeuerGrain>>()
                .ConfigureEndpoints(Dns.GetHostName(), 11111, 40000, listenOnAnyHostAddress: true)
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "squidex";
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("logging"));
                    builder.AddSemanticLog();
                    builder.AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
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

                    services.Configure<ProcessExitHandlingOptions>(options => options.FastKillOnProcessExit = false);
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

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                Task.Run(() => silo.StopAsync()).Wait();
            }
        }
    }
}
