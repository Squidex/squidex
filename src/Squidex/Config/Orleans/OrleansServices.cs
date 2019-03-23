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

            services.AddSingleton<IIncomingGrainCallFilter, LocalCacheFilter>();

            var hostBuilder = new SiloServiceBuilder(config, environment);

            hostBuilder.UseDashboard(options =>
            {
                options.HostSelf = false;
            });

            hostBuilder.ConfigureApplicationParts(builder =>
            {
                builder.AddApplicationPart(SquidexEntities.Assembly);
                builder.AddApplicationPart(SquidexInfrastructure.Assembly);
            });

            var gatewayPort = config.GetOptionalValue("orleans:gatewayPort", 40000);

            var siloPort = config.GetOptionalValue("orleans:siloPort", 11111);

            config.ConfigureByOption("orleans:clustering", new Alternatives
            {
                ["MongoDB"] = () =>
                {
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
                    hostBuilder.UseLocalhostClustering(siloPort, gatewayPort, null, Constants.OrleansClusterId, Constants.OrleansClusterId);
                    hostBuilder.Configure<ClusterMembershipOptions>(options => options.ExpectedClusterSize = 1);
                }
            });

            config.ConfigureByOption("store:type", new Alternatives
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

            hostBuilder.Build(services);

            return services;
        }

        public static void Configure(this ClusterOptions options)
        {
            options.ClusterId = Constants.OrleansClusterId;
            options.ServiceId = Constants.OrleansClusterId;
        }
    }
}
