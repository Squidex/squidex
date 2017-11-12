// ==========================================================================
//  SiloServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime.Configuration;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Orleans;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains;

namespace Squidex.Config.Orleans
{
    public static class SiloServices
    {
        public static void AddAppSiloServices(this IServiceCollection services, IConfiguration config)
        {
            var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
            var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

            var clusterConfiguration =
                services.Where(x => x.ServiceType == typeof(ClusterConfiguration))
                    .Select(x => x.ImplementationInstance)
                    .Select(x => (ClusterConfiguration)x)
                    .FirstOrDefault();

            if (clusterConfiguration != null)
            {
                clusterConfiguration.Globals.RegisterBootstrapProvider<EventConsumerBootstrap>("EventConsumers");
            }

            config.ConfigureByOption("store:type", new Options
            {
                ["MongoDB"] = () =>
                {
                    if (clusterConfiguration != null)
                    {
                        clusterConfiguration.AddMongoDBStorageProvider("Default", c =>
                        {
                            c.ConnectionString = mongoConfiguration;
                            c.CollectionPrefix = "Orleans_";
                            c.DatabaseName = mongoDatabaseName;
                            c.UseJsonFormat = true;
                        });

                        clusterConfiguration.AddMongoDBStatisticsProvider("Default", c =>
                        {
                            c.ConnectionString = mongoConfiguration;
                            c.CollectionPrefix = "Orleans_";
                            c.DatabaseName = mongoDatabaseName;
                        });
                    }

                    services.UseMongoDBGatewayListProvider(c =>
                    {
                        c.ConnectionString = mongoConfiguration;
                        c.CollectionPrefix = "Orleans_";
                        c.DatabaseName = mongoDatabaseName;
                    });

                    services.UseMongoDBMembershipTable(c =>
                    {
                        c.ConnectionString = mongoConfiguration;
                        c.CollectionPrefix = "Orleans_";
                        c.DatabaseName = mongoDatabaseName;
                    });

                    services.UseMongoDBReminders(c =>
                    {
                        c.ConnectionString = mongoConfiguration;
                        c.CollectionPrefix = "Orleans_";
                        c.DatabaseName = mongoDatabaseName;
                    });
                }
            });

            services.AddSingleton<OrleansSiloEventNotifier>()
                .As<IEventNotifier>();
        }
    }
}