// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Samples
{
    public sealed class MemoryAssetStorePlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var storeType = config.GetValue<string>("assetStore:type");

            if (string.Equals(storeType, "Memory", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingletonAs<MemoryAssetStore>()
                    .As<IAssetStore>();
            }
        }
    }
}
