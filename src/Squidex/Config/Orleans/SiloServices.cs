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
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Squidex.Infrastructure.EventSourcing.Grains;

namespace Squidex.Config.Orleans
{
    public static class SiloServices
    {
        public static void AddAppSiloServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs<EventConsumerBootstrap>()
                .As<ILifecycleParticipant<ISiloLifecycle>>();

            /*
            var clusterConfiguration =
                services.Where(x => x.ServiceType == typeof(ClusterConfiguration))
                    .Select(x => x.ImplementationInstance)
                    .Select(x => (ClusterConfiguration)x)
                    .FirstOrDefault();

            if (clusterConfiguration != null)
            {
                clusterConfiguration.Globals.RegisterBootstrapProvider<EventConsumerBootstrap>("EventConsumers");

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
            }*/
        }
    }
}