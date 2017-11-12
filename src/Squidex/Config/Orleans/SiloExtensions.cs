// ==========================================================================
//  SiloExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Orleans.Hosting;

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
    }
}
