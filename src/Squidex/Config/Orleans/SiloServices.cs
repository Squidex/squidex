// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime.Configuration;

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
                var ipConfig = config.GetRequiredValue("orleans:hostNameOrIPAddress");

                if (ipConfig.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    ipConfig = Dns.GetHostName();
                }
                else if (ipConfig.Equals("FirstIPAddressOfHost"))
                {
                    var ips = Dns.GetHostAddressesAsync(Dns.GetHostName()).Result;

                    ipConfig = ips.FirstOrDefault()?.ToString();
                }

                clusterConfiguration.Defaults.PropagateActivityId = true;
                clusterConfiguration.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 40000);
                clusterConfiguration.Defaults.HostNameOrIPAddress = ipConfig;
            }

            config.ConfigureByOption("store:type", new Options
            {
                ["MongoDB"] = () =>
                {
                    var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                    var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

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