// ==========================================================================
//  AssetStoreModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.GoogleCloud;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Azure.Storage;

// ReSharper disable InvertIf

namespace Squidex.Config.Domain
{
    public sealed class AssetStoreModule : Module
    {
        private IConfiguration Configuration { get; }

        public AssetStoreModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var assetStoreType = Configuration.GetValue<string>("assetStore:type");

            if (string.IsNullOrWhiteSpace(assetStoreType))
            {
                throw new ConfigurationException("Configure the AssetStore type with 'assetStore:type'.");
            }

            if (string.Equals(assetStoreType, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                var path = Configuration.GetValue<string>("assetStore:folder:path");

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ConfigurationException("Configure AssetStore Folder path with 'assetStore:folder:path'.");
                }

                builder.Register(c => new FolderAssetStore(path, c.Resolve<ISemanticLog>()))
                    .As<IAssetStore>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }
            else if (string.Equals(assetStoreType, "GoogleCloud", StringComparison.OrdinalIgnoreCase))
            {
                var bucketName = Configuration.GetValue<string>("assetStore:googleCloud:bucket");

                if (string.IsNullOrWhiteSpace(bucketName))
                {
                    throw new ConfigurationException("Configure AssetStore GoogleCloud bucket with 'assetStore:googleCloud:bucket'.");
                }

                builder.Register(c => new GoogleCloudAssetStore(bucketName))
                    .As<IAssetStore>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }
            else if (string.Equals(assetStoreType, "AzureBlobStorage", StringComparison.OrdinalIgnoreCase))
            {
                var containerName = Configuration.GetValue<string>("assetStore:azureStorage:containerName");
                // NOTE: here it can be improved - if we use keyvault the secret key won't be in the app settings, rather the app has to run
                // in the same active directory as the keyvault.
                var connectionString = Configuration.GetValue<string>("assetStore:azureStorage:connectionString");

                if (string.IsNullOrWhiteSpace(containerName))
                {
                    throw new ConfigurationException("Configure AssetStore AzureStorage container with 'assetStore:azureStorage:containerName'.");
                }

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ConfigurationException(
                        "Configure AssetStore AzureStorage connection string with 'assetStore:azureStorage:connectionString'.");
                }

                builder.Register(c => new StorageAccountManager(connectionString))
                    .As<IStorageAccountManager>()
                    .SingleInstance();

                builder.Register(c => new AzureBlobAssetStore(c.Resolve<IStorageAccountManager>(), containerName))
                    .As<IAssetStore>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{assetStoreType}' for 'assetStore:type', supported: Folder, GoogleCloud.");
            }
        }
    }
}
