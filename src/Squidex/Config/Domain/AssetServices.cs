// ==========================================================================
//  AssetServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Log;

namespace Squidex.Config.Domain
{
    public static class AssetServices
    {
        public static void AddMyAssetServices(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("assetStore:type", new Options
            {
                ["Folder"] = () =>
                {
                    var path = config.GetRequiredValue("assetStore:folder:path");

                    services.AddSingleton(c => new FolderAssetStore(path, c.GetRequiredService<ISemanticLog>()))
                        .As<IAssetStore>()
                        .As<IExternalSystem>();
                },
                ["GoogleCloud"] = () =>
                {
                    var bucketName = config.GetRequiredValue("assetStore:googleCloud:bucket");

                    services.AddSingleton(c => new GoogleCloudAssetStore(bucketName))
                        .As<IAssetStore>()
                        .As<IExternalSystem>();
                },
                ["AzureBlob"] = () =>
                {
                    var connectionString = config.GetRequiredValue("assetStore:azureBlob:connectionString");
                    var containerName = config.GetRequiredValue("assetStore:azureBlob:containerName");

                    services.AddSingleton(c => new AzureBlobAssetStore(connectionString, containerName))
                        .As<IAssetStore>()
                        .As<IExternalSystem>();
                }
            });
        }
    }
}
