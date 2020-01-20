﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Queries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Assets.ImageSharp;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;

namespace Squidex.Config.Domain
{
    public static class AssetServices
    {
        public static void AddSquidexAssets(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AssetOptions>(
                config.GetSection("assets"));

            services.AddTransientAs<AssetDomainObject>()
                .AsSelf();

            services.AddTransientAs<AssetFolderDomainObject>()
                .AsSelf();

            services.AddSingletonAs<AssetQueryParser>()
                .AsSelf();

            services.AddSingletonAs<DefaultAssetFileStore>()
                .As<IAssetFileStore>();

            services.AddSingletonAs<AssetEnricher>()
                .As<IAssetEnricher>();

            services.AddSingletonAs<AssetQueryService>()
                .As<IAssetQueryService>();

            services.AddSingletonAs<AssetLoader>()
                .As<IAssetLoader>();

            services.AddSingletonAs<AssetUsageTracker>()
                .As<IAssetUsageTracker>().As<IEventConsumer>();

            services.AddSingletonAs<FileTypeTagGenerator>()
                .As<IAssetMetadataSource>();

            services.AddSingletonAs<FileTagAssetMetadataSource>()
                .As<IAssetMetadataSource>();
        }

        public static void AddSquidexAssetInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("assetStore:type", new Alternatives
            {
                ["Default"] = () =>
                {
                    services.AddSingletonAs<NoopAssetStore>()
                        .AsOptional<IAssetStore>();
                },
                ["Folder"] = () =>
                {
                    var path = config.GetRequiredValue("assetStore:folder:path");

                    services.AddSingletonAs(c => new FolderAssetStore(path, c.GetRequiredService<ISemanticLog>()))
                        .As<IAssetStore>();
                },
                ["GoogleCloud"] = () =>
                {
                    var bucketName = config.GetRequiredValue("assetStore:googleCloud:bucket");

                    services.AddSingletonAs(c => new GoogleCloudAssetStore(bucketName))
                        .As<IAssetStore>();
                },
                ["AzureBlob"] = () =>
                {
                    var connectionString = config.GetRequiredValue("assetStore:azureBlob:connectionString");
                    var containerName = config.GetRequiredValue("assetStore:azureBlob:containerName");

                    services.AddSingletonAs(c => new AzureBlobAssetStore(connectionString, containerName))
                        .As<IAssetStore>();
                },
                ["AmazonS3"] = () =>
                {
                    var amazonS3Options = config.GetSection("assetStore:amazonS3").Get<AmazonS3Options>();

                    services.AddSingletonAs(c => new AmazonS3AssetStore(amazonS3Options))
                        .As<IAssetStore>();
                },
                ["MongoDb"] = () =>
                {
                    var mongoConfiguration = config.GetRequiredValue("assetStore:mongoDb:configuration");
                    var mongoDatabaseName = config.GetRequiredValue("assetStore:mongoDb:database");
                    var mongoGridFsBucketName = config.GetRequiredValue("assetStore:mongoDb:bucket");

                    services.AddSingletonAs(c =>
                        {
                            var mongoClient = Singletons<IMongoClient>.GetOrAdd(mongoConfiguration, s => new MongoClient(s));
                            var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

                            var gridFsbucket = new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
                            {
                                BucketName = mongoGridFsBucketName
                            });

                            return new MongoGridFsAssetStore(gridFsbucket);
                        })
                        .As<IAssetStore>();
                },
                ["Ftp"] = () =>
                {
                    var serverHost = config.GetRequiredValue("assetStore:ftp:serverHost");
                    var serverPort = config.GetOptionalValue<int>("assetStore:ftp:serverPort", 21);

                    var username = config.GetRequiredValue("assetStore:ftp:username");
                    var password = config.GetRequiredValue("assetStore:ftp:password");

                    var path = config.GetOptionalValue("assetStore:ftp:path", "/");

                    services.AddSingletonAs(c =>
                        {
                            var factory = new Func<FtpClient>(() => new FtpClient(serverHost, serverPort, username, password));

                            return new FTPAssetStore(factory, path, c.GetRequiredService<ISemanticLog>());
                        })
                        .As<IAssetStore>();
                }
            });

            services.AddSingletonAs<ImageSharpAssetThumbnailGenerator>()
                .As<IAssetThumbnailGenerator>();
        }
    }
}
