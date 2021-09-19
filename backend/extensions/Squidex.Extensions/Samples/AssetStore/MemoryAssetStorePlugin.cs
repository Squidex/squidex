// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Assets;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Samples.AssetStore
{
    public sealed class MemoryAssetStorePlugin : IPlugin, IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.Use(async (context, next) =>
                {
                    if (context.Request.Path.StartsWithSegments("/api/assets/memory", StringComparison.Ordinal))
                    {
                        context.Response.StatusCode = 200;

                        await context.Response.WriteAsync("Memory Asset Store used.");
                    }
                    else
                    {
                        await next();
                    }
                });

                next(builder);
            };
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var storeType = config.GetValue<string>("assetStore:type");

            var isMemoryAssetsUsed = string.Equals(storeType, "Memory", StringComparison.OrdinalIgnoreCase);

            if (isMemoryAssetsUsed)
            {
                services.AddSingleton<IStartupFilter>(this);

                services.AddSingleton<IAssetStore, MemoryAssetStore>();
            }
        }
    }
}
