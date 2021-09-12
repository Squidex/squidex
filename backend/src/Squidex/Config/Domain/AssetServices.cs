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
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Assets;
using Squidex.Assets.ImageSharp;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Assets.Queries;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain
{
    public static class AssetServices
    {
        public static void AddSquidexAssets(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AssetOptions>(config,
                "assets");

            if (config.GetValue<bool>("assets:deleteRecursive"))
            {
                services.AddTransientAs<RecursiveDeleter>()
                   .As<IEventConsumer>();
            }

            if (config.GetValue<bool>("assets:deletePermanent"))
            {
                services.AddTransientAs<AssetPermanentDeleter>()
                   .As<IEventConsumer>();
            }

            services.AddTransientAs<AssetDomainObject>()
                .AsSelf();

            services.AddTransientAs<AssetFolderDomainObject>()
                .AsSelf();

            services.AddSingletonAs<AssetQueryParser>()
                .AsSelf();

            services.AddSingletonAs<RebuildFiles>()
                .AsSelf();

            services.AddTransientAs<AssetTagsDeleter>()
                .As<IDeleter>();

            services.AddTransientAs<AssetHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<AssetsSearchSource>()
                .As<ISearchSource>();

            services.AddSingletonAs<DefaultAssetFileStore>()
                .As<IAssetFileStore>().As<IDeleter>();

            services.AddSingletonAs<AssetEnricher>()
                .As<IAssetEnricher>();

            services.AddSingletonAs<AssetQueryService>()
                .As<IAssetQueryService>();

            services.AddSingletonAs<AssetLoader>()
                .As<IAssetLoader>();

            services.AddSingletonAs<AssetUsageTracker>()
                .As<IAssetUsageTracker>().As<IEventConsumer>().As<IDeleter>();

            services.AddSingletonAs<FileTypeAssetMetadataSource>()
                .As<IAssetMetadataSource>();

            services.AddSingletonAs<FileTagAssetMetadataSource>()
                .As<IAssetMetadataSource>();

            services.AddSingletonAs<ImageAssetMetadataSource>()
                .As<IAssetMetadataSource>();

            services.AddSingletonAs<SvgAssetMetadataSource>()
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

                    services.AddSingletonAs(c => new FolderAssetStore(path, c.GetRequiredService<ILogger<FolderAssetStore>>()))
                        .As<IAssetStore>();
                },
                ["GoogleCloud"] = () =>
                {
                    var options = new GoogleCloudAssetOptions
                    {
                        BucketName = config.GetRequiredValue("assetStore:googleCloud:bucket")
                    };

                    services.AddSingletonAs(c => new GoogleCloudAssetStore(options))
                        .As<IAssetStore>();
                },
                ["AzureBlob"] = () =>
                {
                    var options = new AzureBlobAssetOptions
                    {
                        ConnectionString = config.GetRequiredValue("assetStore:azureBlob:connectionString"),
                        ContainerName = config.GetRequiredValue("assetStore:azureBlob:containerName")
                    };

                    services.AddSingletonAs(c => new AzureBlobAssetStore(options))
                        .As<IAssetStore>();
                },
                ["AmazonS3"] = () =>
                {
                    var amazonS3Options = config.GetSection("assetStore:amazonS3").Get<AmazonS3AssetOptions>() ?? new ();

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

                    var options = new FTPAssetOptions
                    {
                        Path = config.GetOptionalValue("assetStore:ftp:path", "/")
                    };

                    services.AddSingletonAs(c =>
                        {
                            var factory = new Func<FtpClient>(() => new FtpClient(serverHost, serverPort, username, password));

                            return new FTPAssetStore(factory, options, c.GetRequiredService<ILogger<FTPAssetStore>>());
                        })
                        .As<IAssetStore>();
                }
            });

            services.AddSingletonAs<ImageSharpAssetThumbnailGenerator>()
                .As<IAssetThumbnailGenerator>();

            services.AddSingletonAs(c => new DelegateInitializer(
                    c.GetRequiredService<IAssetStore>().GetType().Name,
                    c.GetRequiredService<IAssetStore>().InitializeAsync))
                .As<IInitializable>();
        }
    }
}
