// ==========================================================================
//  SiloServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

namespace Squidex.Config.Orleans
{
    public static class SiloServices
    {
        public static IServiceCollection AddOrleans(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            var properties = new Dictionary<object, object>();

            var hostBuilderContext = new HostBuilderContext(new Dictionary<object, object>())
            {
                Configuration = configuration
            };

            var orleansConfig = configuration.GetSection("orleans");

            var clusterConfiguration = ClusterConfiguration.LocalhostPrimarySilo(33333);
            clusterConfiguration.AddMongoDBStatisticsProvider("Default", orleansConfig);
            clusterConfiguration.AddMongoDBStorageProvider("Default", orleansConfig);

            services.AddD

            return services;
        }
    }
}
