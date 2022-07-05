// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Providers.MongoDB.Utils;
using Orleans.Runtime;
using OrleansDashboard;
using Squidex.Domain.Apps.Entities;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Web;

namespace Squidex.Config.Orleans
{
    public static class OrleansServices
    {
        public static void ConfigureForSquidex(this ISiloBuilder builder, IConfiguration config)
        {
            builder.AddOrleansPubSub();

            builder.ConfigureServices(services =>
            {
                services.AddScoped(typeof(IGrainState<>), typeof(Infrastructure.Orleans.GrainState<>));

                services.AddSingletonAs<DefaultMongoClientFactory>()
                    .As<IMongoClientFactory>();

                services.AddSingletonAs<ActivationLimiter>()
                    .As<IActivationLimiter>();

                services.AddScopedAs<ActivationLimit>()
                    .As<IActivationLimit>();

                services.AddScoped(x => x.GetRequiredService<IGrainActivationContext>().GrainIdentity);
            });

            builder.ConfigureApplicationParts(parts =>
            {
                parts.AddApplicationPart(SquidexEntities.Assembly);
                parts.AddApplicationPart(SquidexInfrastructure.Assembly);
            });

            builder.Configure<SerializationProviderOptions>(options =>
            {
                options.SerializationProviders.Add(typeof(JsonSerializer));
            });

            builder.Configure<SchedulingOptions>(options =>
            {
                options.TurnWarningLengthThreshold = TimeSpan.FromSeconds(5);
            });

            builder.Configure<ClusterOptions>(options =>
            {
                options.Configure();
            });

            builder.Configure<DashboardOptions>(options =>
            {
                options.HideTrace = true;
            });

            builder.UseDashboardEmbeddedFiles();
            builder.UseDashboard(options =>
            {
                options.HostSelf = false;
            });

            builder.AddOutgoingGrainCallFilter<ActivityPropagationFilter>();
            builder.AddOutgoingGrainCallFilter<CultureFilter>();
            builder.AddIncomingGrainCallFilter<ExceptionWrapperFilter>();
            builder.AddIncomingGrainCallFilter<ActivityPropagationFilter>();
            builder.AddIncomingGrainCallFilter<ActivationLimiterFilter>();
            builder.AddIncomingGrainCallFilter<CultureFilter>();
            builder.AddIncomingGrainCallFilter<LocalCacheFilter>();
            builder.AddIncomingGrainCallFilter<LoggingFilter>();
            builder.AddIncomingGrainCallFilter<StateFilter>();

            var (siloPort, gatewayPort) = GetPorts(config);

            config.ConfigureByOption("orleans:clustering", new Alternatives
            {
                ["MongoDB"] = () =>
                {
                    var address = GetIPAddress(config);

                    builder.ConfigureEndpoints(
                        address,
                        siloPort,
                        gatewayPort,
                        true);

                    builder.UseMongoDBClustering(options =>
                    {
                        options.Strategy = MongoDBMembershipStrategy.SingleDocument;

                        options.Configure(config);
                    });

                    if (config.GetValue<bool>("orleans:kubernetes"))
                    {
                        builder.UseKubernetesHosting();
                    }
                },
                ["Development"] = () =>
                {
                    builder.UseLocalhostClustering(siloPort, gatewayPort);
                }
            });

            config.ConfigureByOption("store:type", new Alternatives
            {
                ["MongoDB"] = () =>
                {
                    builder.UseMongoDBReminders(options =>
                    {
                        options.Configure(config);
                    });
                }
            });
        }

        private static (int, int) GetPorts(IConfiguration config)
        {
            var orleansPortSilo = config.GetOptionalValue("orleans:siloPort", 11111);
            var orleansPortGateway = config.GetOptionalValue("orleans:gatewayPort", 40000);

            var privatePorts = config.GetOptionalValue("WEBSITE_PRIVATE_PORTS", string.Empty);

            if (!string.IsNullOrWhiteSpace(privatePorts) && config.GetValue<bool>("orleans:useAzureNetwork"))
            {
                var ports = privatePorts.Split(',');

                if (ports.Length < 1 || !int.TryParse(ports[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out orleansPortSilo))
                {
                    var error = new ConfigurationError("Insufficient private ports configured.", "WEBSITE_PRIVATE_PORTS");

                    throw new ConfigurationException(error);
                }
            }

            return (orleansPortSilo, orleansPortGateway);
        }

        private static IPAddress GetIPAddress(IConfiguration config)
        {
            IPAddress? address = null;

            var configuredAddress = config.GetOptionalValue("orleans:ipAddress", string.Empty);

            if (!string.IsNullOrWhiteSpace(configuredAddress))
            {
                address = IPAddress.Parse(configuredAddress);
            }

            if (address == null && config.GetValue<bool>("orleans:useAzureNetwork"))
            {
                var privateIP = config.GetOptionalValue("WEBSITE_PRIVATE_IP", string.Empty);

                if (!string.IsNullOrWhiteSpace(privateIP))
                {
                    address = IPAddress.Parse(privateIP);
                }
            }

            address ??= Helper.ResolveIPAddressAsync(Dns.GetHostName(), AddressFamily.InterNetwork).Result;

            return address;
        }

        private static void Configure(this MongoDBOptions options, IConfiguration config)
        {
            options.CollectionPrefix = "Orleans_";

            options.DatabaseName = config.GetRequiredValue("store:mongoDb:database");
        }

        private static void Configure(this ClusterOptions options)
        {
            options.ClusterId = Constants.OrleansClusterId;
            options.ServiceId = Constants.OrleansClusterId;
        }
    }
}
