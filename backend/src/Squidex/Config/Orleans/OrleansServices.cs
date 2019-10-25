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
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using OrleansDashboard;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Web;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Squidex.Config.Orleans
{
    public static class OrleansServices
    {
        public static void ConfigureForSquidex(this ISiloBuilder builder, IConfiguration config)
        {
            builder.ConfigureServices(siloServices =>
            {
                siloServices.AddSingleton<IActivationLimiter, ActivationLimiter>();
                siloServices.AddScoped<IActivationLimit, ActivationLimit>();

                siloServices.AddScoped(typeof(IGrainState<>), typeof(Squidex.Infrastructure.Orleans.GrainState<>));
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

            builder.AddIncomingGrainCallFilter<ActivationLimiterFilter>();
            builder.AddIncomingGrainCallFilter<LocalCacheFilter>();
            builder.AddIncomingGrainCallFilter<LoggingFilter>();
            builder.AddIncomingGrainCallFilter<StateFilter>();

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
