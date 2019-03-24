// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansDashboard;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
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
                    siloServices.Configure<ClusterOptions>(options =>
                    {
                        options.Configure();
                    });

                    siloServices.Configure<ProcessExitHandlingOptions>(options =>
                    {
                        options.FastKillOnProcessExit = false;
                    });

                    siloServices.Configure<DashboardOptions>(options =>
                    {
                        options.HideTrace = true;
                    });

                    siloServices.AddSingleton<IIncomingGrainCallFilter, LocalCacheFilter>();
                });

                builder.ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(SquidexEntities.Assembly);
                    parts.AddApplicationPart(SquidexInfrastructure.Assembly);
                });

                builder.UseDashboard(options =>
                {
                    options.HostSelf = false;
                });

                var gatewayPort = config.GetOptionalValue("orleans:gatewayPort", 40000);

                var siloPort = config.GetOptionalValue("orleans:siloPort", 11111);

                config.ConfigureByOption("orleans:clustering", new Alternatives
                {
                    ["MongoDB"] = () =>
                    {
                        builder.ConfigureEndpoints(Dns.GetHostName(), siloPort, gatewayPort, listenOnAnyHostAddress: true);

                        var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                        var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

                        builder.UseMongoDBClustering(options =>
                        {
                            options.ConnectionString = mongoConfiguration;
                            options.CollectionPrefix = "Orleans_";
                            options.DatabaseName = mongoDatabaseName;
                        });
                    },
                    ["Development"] = () =>
                    {
                        builder.UseLocalhostClustering(siloPort, gatewayPort, null, Constants.OrleansClusterId, Constants.OrleansClusterId);
                        builder.Configure<ClusterMembershipOptions>(options => options.ExpectedClusterSize = 1);
                    }
                });

                config.ConfigureByOption("store:type", new Alternatives
                {
                    ["MongoDB"] = () =>
                    {
                        var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                        var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");

                        builder.UseMongoDBReminders(options =>
                        {
                            options.ConnectionString = mongoConfiguration;
                            options.CollectionPrefix = "Orleans_";
                            options.DatabaseName = mongoDatabaseName;
                        });
                    }
                });
            });

            return services;
        }

        public static void Configure(this ClusterOptions options)
        {
            options.ClusterId = Constants.OrleansClusterId;
            options.ServiceId = Constants.OrleansClusterId;
        }
    }
}
