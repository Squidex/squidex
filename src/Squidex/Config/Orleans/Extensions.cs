// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansDashboard;
using OrleansDashboard.Client;
using OrleansDashboard.Metrics;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;

namespace Squidex.Config.Orleans
{
    public static class Extensions
    {
        public static void AddMyParts(this IApplicationPartManager builder)
        {
            builder.AddApplicationPart(SquidexEntities.Assembly);
            builder.AddApplicationPart(SquidexInfrastructure.Assembly);
        }

        public static void Configure(this ClusterOptions options)
        {
            options.ClusterId = Constants.OrleansClusterId;
            options.ServiceId = Constants.OrleansClusterId;
        }

        public static ISiloHostBuilder UseDashboardEx(this ISiloHostBuilder builder, Action<DashboardOptions> configurator = null)
        {
            builder.AddStartupTask<Dashboard>();

            builder.ConfigureApplicationParts(appParts =>
                appParts
                    .AddFrameworkPart(typeof(Dashboard).Assembly)
                    .AddFrameworkPart(typeof(DashboardClient).Assembly));

            builder.ConfigureServices(services =>
            {
                services.AddDashboard(options =>
                {
                    options.HostSelf = false;
                });
            });

            builder.AddIncomingGrainCallFilter<GrainProfiler>();

            return builder;
        }
    }
}
