// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Samples.AssetStore
{
    public sealed class FTPAssetStorePlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var storeType = config.GetValue<string>("assetStore:type");

            if (string.Equals(storeType, "FTP", StringComparison.OrdinalIgnoreCase))
            {
                var host = config.GetRequiredValue("assetStore:ftp:host");
                var port = config.GetOptionalValue<int>("assetStore:ftp:port", 21);
                var username = config.GetRequiredValue("assetStore:ftp:username");
                var password = config.GetRequiredValue("assetStore:ftp:password");
                var path = config.GetOptionalValue("assetStore:ftp:path", "/");

                services.AddSingletonAs(c => new FTPAssetStore(host, port, username, password, path)).As<IAssetStore>();
            }
        }
    }
}
