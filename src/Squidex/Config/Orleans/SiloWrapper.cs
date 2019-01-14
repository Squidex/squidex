// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Log.Adapter;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Config.Orleans
{
    public sealed class SiloWrapper : IHostedService
    {
        private readonly Lazy<ISiloHost> lazySilo;
        private readonly ISemanticLog log;
        private readonly IApplicationLifetime lifetime;
        private bool isStopping;

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

        public IClusterClient Client
        {
            get { return lazySilo.Value.Services.GetRequiredService<IClusterClient>(); }
        }

        public SiloWrapper(IConfiguration config, ISemanticLog log, IApplicationLifetime lifetime)
        {
            this.lifetime = lifetime;
            this.log = log;

            lazySilo = new Lazy<ISiloHost>(() =>
            {
                var hostBuilder = new SiloHostBuilder()
                    .UseDashboard(options => options.HostSelf = false)
                    .EnableDirectClient()
                    .AddIncomingGrainCallFilter<LocalCacheFilter>()
                    .AddStartupTask<InitializerStartup>()
                    .AddStartupTask<Bootstrap<IContentSchedulerGrain>>()
                    .AddStartupTask<Bootstrap<IEventConsumerManagerGrain>>()
                    .AddStartupTask<Bootstrap<IRuleDequeuerGrain>>()
                    .AddStartupTask<Bootstrap<IUsageTrackerGrain>>()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.Configure();
                    })
                    .ConfigureApplicationParts(builder =>
                    {
                        builder.AddMyParts();
                    })
                    .ConfigureLogging((hostingContext, builder) =>
                    {
                        builder.AddConfiguration(hostingContext.Configuration.GetSection("logging"));
                        builder.AddSemanticLog();
                        builder.AddFilter();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddAppSiloServices(context.Configuration);
                        services.AddAppServices(context.Configuration);

                        services.Configure<ProcessExitHandlingOptions>(options => options.FastKillOnProcessExit = false);
                    })
                    .ConfigureAppConfiguration((hostContext, builder) =>
                    {
                        if (config is IConfigurationRoot root)
                        {
                            foreach (var provider in root.Providers)
                            {
                                builder.Add(new Source(provider));
                            }
                        }
                    });

                config.ConfigureByOption("orleans:clustering", new Options
                {
                    ["MongoDB"] = () =>
                    {
                        hostBuilder.ConfigureEndpoints(Dns.GetHostName(), 11111, 40000, listenOnAnyHostAddress: true);

                        var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                        var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

                        hostBuilder.UseMongoDBClustering(options =>
                        {
                            options.ConnectionString = mongoConfiguration;
                            options.CollectionPrefix = "Orleans_";
                            options.DatabaseName = mongoDatabaseName;
                        });
                    },
                    ["Development"] = () =>
                    {
                        hostBuilder.UseLocalhostClustering(gatewayPort: 40000, serviceId: Constants.OrleansClusterId, clusterId: Constants.OrleansClusterId);
                        hostBuilder.Configure<ClusterMembershipOptions>(options => options.ExpectedClusterSize = 1);
                    }
                });

                config.ConfigureByOption("store:type", new Options
                {
                    ["MongoDB"] = () =>
                    {
                        var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                        var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

                        hostBuilder.UseMongoDBReminders(options =>
                        {
                            options.ConnectionString = mongoConfiguration;
                            options.CollectionPrefix = "Orleans_";
                            options.DatabaseName = mongoDatabaseName;
                        });
                    }
                });

                var silo = hostBuilder.Build();

                silo.Stopped.ContinueWith(x =>
                {
                    if (!isStopping)
                    {
                        lifetime.StopApplication();
                    }
                });

                return silo;
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                await lazySilo.Value.StartAsync(cancellationToken);
            }
            catch
            {
                lifetime.StopApplication();
                throw;
            }
            finally
            {
                var elapsedMs = watch.Stop();

                log.LogInformation(w => w
                    .WriteProperty("message", "Silo started")
                    .WriteProperty("elapsedMs", elapsedMs));
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (lazySilo.IsValueCreated)
            {
                isStopping = true;

                await lazySilo.Value.StopAsync(cancellationToken);
            }
        }
    }
}
