// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

namespace Squidex.Config.Orleans
{
    public static class SiloExtensions
    {
        public static ISiloHostBuilder UseContentRoot(this ISiloHostBuilder builder, string path)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(path);
            });

            return builder;
        }

        public static ClusterConfiguration WithDashboard(this ClusterConfiguration config)
        {
            config.RegisterDashboard();

            return config;
        }
    }
}
