// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Config.Orleans
{
    public static class OrleansServices
    {
        public static IServiceProvider AddAndBuildOrleans(this IServiceCollection services, IConfiguration config, Action<IServiceCollection> afterServices)
        {
            services.Configure<ClusterOptions>(options =>
            {
                options.Configure();
            });

            services.Configure<ProcessExitHandlingOptions>(options =>
            {
                options.FastKillOnProcessExit = false;
            });

            services.AddServicesForSelfHostedDashboard(null, options =>
            {
                options.HideTrace = true;
            });

            services.AddHostedService<SiloHost>();

            var hostBuilder = new SiloHostBuilder()
                .UseDashboardEx()
                .EnableDirectClient()
                .AddIncomingGrainCallFilter<LocalCacheFilter>()
                .AddStartupTask<Bootstrap<IContentSchedulerGrain>>()
                .AddStartupTask<Bootstrap<IEventConsumerManagerGrain>>()
                .AddStartupTask<Bootstrap<IRuleDequeuerGrain>>()
                .AddStartupTask<Bootstrap<IUsageTrackerGrain>>()
                .ConfigureApplicationParts(builder =>
                {
                    builder.AddMyParts();
                });

            config.ConfigureByOption("orleans:clustering", new Options
            {
                ["MongoDB"] = () =>
                {
                    var siloPort = int.Parse(Environment.GetEnvironmentVariable("Orleans.SiloPort") ?? "11111");
                    var gatewayPort = int.Parse(Environment.GetEnvironmentVariable("Orleans.GatewayPort") ?? "40000");

                    hostBuilder.ConfigureEndpoints(Dns.GetHostName(), siloPort, gatewayPort, listenOnAnyHostAddress: true);

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

            IServiceProvider provider = null;

            hostBuilder.UseServiceProviderFactory((siloServices) =>
            {
                foreach (var descriptor in services)
                {
                    siloServices.Add(descriptor);
                }

                afterServices(siloServices);

                provider = siloServices.BuildServiceProvider();

                return provider;
            }).Build();

            return provider;
        }
    }
}
