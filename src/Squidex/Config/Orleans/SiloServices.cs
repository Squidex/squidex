// ==========================================================================
//  SiloServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime.Configuration;
using Squidex.Domain.Apps.Read.Rules.Orleans;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains;

namespace Squidex.Config.Orleans
{
    public static class SiloServices
    {
        public static void AddAppSiloServices(this IServiceCollection services, IConfiguration config)
        {
            var clusterConfiguration =
                services.Where(x => x.ServiceType == typeof(ClusterConfiguration))
                    .Select(x => x.ImplementationInstance)
                    .Select(x => (ClusterConfiguration)x)
                    .FirstOrDefault();

            if (clusterConfiguration != null)
            {
                clusterConfiguration.Globals.RegisterBootstrapProvider<EventConsumerBootstrap>("EventConsumers");
                clusterConfiguration.Globals.RegisterBootstrapProvider<RuleDequeuerBootstrap>("RuleDequeuer");

                var ipConfig = config.GetRequiredValue("orleans:hostNameOrIPAddress");

                if (ipConfig.Equals("FirstOfHost"))
                {
                    var ips = Dns.GetHostAddressesAsync(Dns.GetHostName()).Result;

                    ipConfig = ips.FirstOrDefault()?.ToString();
                }

                clusterConfiguration.Defaults.PropagateActivityId = true;
                clusterConfiguration.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 10400);
                clusterConfiguration.Defaults.HostNameOrIPAddress = ipConfig;
            }

            config.ConfigureByOption("store:type", new Options
            {
                ["MongoDB"] = () =>
                {
                    var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                    var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

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