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
using Squidex.Infrastructure.Log;

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
            else
            {
                throw new ConfigurationException($"Unsupported value '{assetStoreType}' for 'assetStore:type', supported: Folder.");
            }
        }
    }
}
