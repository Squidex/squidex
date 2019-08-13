// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using OrleansDashboard;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Web;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Squidex.Config.Orleans
{
    public static class OrleansServices
    {
        public static IServiceCollection AddOrleans(this IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
        {
            services.AddOrleans(config, environment, builder =>
            {
                builder.ConfigureServices(siloServices =>
                {
                    siloServices.AddSingleton<IIncomingGrainCallFilter, LocalCacheFilter>();
                    siloServices.AddSingleton<IIncomingGrainCallFilter, LoggingFilter>();
                });

                builder.ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(SquidexEntities.Assembly);
                    parts.AddApplicationPart(SquidexInfrastructure.Assembly);
                });

                builder.Configure<ClusterOptions>(options =>
                {
                    options.Configure();
                });

                builder.Configure<ProcessExitHandlingOptions>(options =>
                {
                    options.FastKillOnProcessExit = false;
                });

                builder.Configure<DashboardOptions>(options =>
                {
                    options.HideTrace = true;
                });

                builder.UseDashboard(options =>
                {
                    options.HostSelf = false;
                });

                var orleansPortSilo = config.GetOptionalValue("orleans:siloPort", 11111);
                var orleansPortGateway = config.GetOptionalValue("orleans:gatewayPort", 40000);

                var address = Helper.ResolveIPAddressAsync(Dns.GetHostName(), AddressFamily.InterNetwork).Result;

                builder.ConfigureEndpoints(
                    address,
                    orleansPortSilo,
                    orleansPortGateway,
                    true);

                config.ConfigureByOption("orleans:clustering", new Alternatives
                {
                    ["MongoDB"] = () =>
                    {
                        builder.UseMongoDBClustering(options =>
                        {
                            options.Configure(config);
                        });
                    },
                    ["Development"] = () =>
                    {
                        builder.UseDevelopmentClustering(new IPEndPoint(address, orleansPortSilo));

                        builder.Configure<ClusterMembershipOptions>(options =>
                        {
                            options.ExpectedClusterSize = 1;
                        });
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
            });

            return services;
        }

        private static void Configure(this MongoDBOptions options, IConfiguration config)
        {
            var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
            var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

            options.ConnectionString = mongoConfiguration;
            options.CollectionPrefix = "Orleans_";
            options.DatabaseName = mongoDatabaseName;
        }

        private static void Configure(this ClusterOptions options)
        {
            options.ClusterId = Constants.OrleansClusterId;
            options.ServiceId = Constants.OrleansClusterId;
        }
    }
}
