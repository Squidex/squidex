// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Queries;
using Squidex.Domain.Apps.Entities.Assets.Queries.Steps;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain;

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

        services.AddSingletonAs<AssetQueryParser>()
            .AsSelf();

        services.AddTransientAs<AssetTagsDeleter>()
            .As<IDeleter>();

        services.AddSingletonAs<AssetCache>()
            .As<IAssetCache>();

        services.AddSingletonAs<RebuildFiles>()
            .AsSelf();

        services.AddTransientAs<AssetHistoryEventsCreator>()
            .As<IHistoryEventsCreator>();

        services.AddSingletonAs<AssetsSearchSource>()
            .As<ISearchSource>();

        services.AddSingletonAs<DefaultAssetFileStore>()
            .As<IAssetFileStore>().As<IDeleter>();

        services.AddSingletonAs<AssetEnricher>()
            .As<IAssetEnricher>();

        services.AddSingletonAs<CalculateTokens>()
            .As<IAssetEnricherStep>();

        services.AddSingletonAs<ConvertTags>()
            .As<IAssetEnricherStep>();

        services.AddSingletonAs<EnrichForCaching>()
            .As<IAssetEnricherStep>();

        services.AddSingletonAs<EnrichWithMetadataText>()
            .As<IAssetEnricherStep>();

        services.AddSingletonAs<ScriptAsset>()
            .As<IAssetEnricherStep>();

        services.AddSingletonAs<AssetQueryService>()
            .As<IAssetQueryService>();

        services.AddSingletonAs<AssetLoader>()
            .As<IAssetLoader>();

        services.AddSingletonAs<AssetUsageTracker>()
            .As<IEventConsumer>().As<IDeleter>();

        services.AddSingletonAs<FileTypeAssetMetadataSource>()
            .As<IAssetMetadataSource>();

        services.AddSingletonAs<FileTagAssetMetadataSource>()
            .As<IAssetMetadataSource>();

        services.AddSingletonAs<ImageAssetMetadataSource>()
            .As<IAssetMetadataSource>();

        services.AddSingletonAs<SvgAssetMetadataSource>()
            .As<IAssetMetadataSource>();

        services.AddAssetTus();
    }

    public static void AddSquidexAssetInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("assetStore:type", new Alternatives
        {
            ["Default"] = () =>
            {
                services.AddFolderAssetStore(config);
            },
            ["Folder"] = () =>
            {
                services.AddFolderAssetStore(config);
            },
            ["GoogleCloud"] = () =>
            {
                services.AddGoogleCloudAssetStore(config);
            },
            ["AzureBlob"] = () =>
            {
                services.AddAzureBlobAssetStore(config);
            },
            ["AmazonS3"] = () =>
            {
                services.AddAmazonS3AssetStore(config);
            },
            ["FTP"] = () =>
            {
                services.AddFTPAssetStore(config);
            },
            ["MongoDb"] = () =>
            {
                var mongoConfiguration = config.GetRequiredValue("assetStore:mongoDb:configuration");
                var mongoDatabaseName = config.GetRequiredValue("assetStore:mongoDb:database");
                var mongoGridFsBucketName = config.GetRequiredValue("assetStore:mongoDb:bucket");

                services.AddMongoAssetStore(c =>
                {
                    var mongoClient = StoreServices.GetMongoClient(mongoConfiguration);
                    var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

                    return new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
                    {
                        BucketName = mongoGridFsBucketName
                    });
                });
            }
        });
    }
}
