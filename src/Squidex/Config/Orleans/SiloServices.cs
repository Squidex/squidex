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
                        clusterConfiguration.AddMongoDBStorageProvider<CustomMongoDbStorageProvider>("Default", c =>
                        {
                            c.ConnectionString = mongoConfiguration;
                            c.CollectionPrefix = "States_";
                            c.DatabaseName = mongoDatabaseName;
                            c.UseJsonFormat = true;
                        });
                    }

                    services.AddMongoDBMembershipTable(c =>
                    {
                        c.ConnectionString = mongoConfiguration;
                        c.CollectionPrefix = "Orleans_";
                        c.DatabaseName = mongoDatabaseName;
                    });

                    services.AddMongoDBReminders(c =>
                    {
                        c.ConnectionString = mongoConfiguration;
                        c.CollectionPrefix = "Orleans_";
                        c.DatabaseName = mongoDatabaseName;
                    });
                }
            });
        }
    }
}