// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using MongoDB.Driver.GridFS;
using Squidex.AI;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Domain.Users;
using Squidex.Events;
using Squidex.Events.Mongo;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Migrations;
using Squidex.Migrations.Backup;
using YDotNet.Server.MongoDB;

namespace Squidex;

public static class ServiceExtensions
{
    public static void AddSquidexMongoEventStore(this IServiceCollection services, IConfiguration config)
    {
        var mongoConfiguration = config.GetRequiredValue("eventStore:mongoDb:configuration");
        var mongoDatabaseName = config.GetRequiredValue("eventStore:mongoDb:database");

        services.AddMongoEventStore(config);
        services.AddSingletonAs(c =>
        {
            var options = c.GetRequiredService<IOptions<MongoEventStoreOptions>>();
            var mongoClient = GetMongoClient(mongoConfiguration);
            var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

            return new MongoEventStore(mongoDatabase, options);
        })
        .As<IEventStore>();
    }

    public static void AddSquidexMongoAssetStore(this IServiceCollection services, IConfiguration config)
    {
        var mongoConfiguration = config.GetRequiredValue("assetStore:mongoDb:configuration");
        var mongoDatabaseName = config.GetRequiredValue("assetStore:mongoDb:database");
        var mongoGridFsBucketName = config.GetRequiredValue("assetStore:mongoDb:bucket");

        services.AddMongoAssetStore(c =>
        {
            var mongoClient = GetMongoClient(mongoConfiguration);
            var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

            return new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
            {
                BucketName = mongoGridFsBucketName,
            });
        });
    }

    public static void AddSquidexMongoStore(this IServiceCollection services, IConfiguration config)
    {
        var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration")!;
        var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database")!;
        var mongoContentDatabaseName = config.GetOptionalValue("store:mongoDb:contentDatabase", mongoDatabaseName)!;

        var contentDatabase = new Func<IServiceProvider, IMongoDatabase>(c =>
        {
            return GetDatabase(c, mongoDatabaseName);
        });

        services.AddSingletonAs(c => GetMongoClient(mongoConfiguration))
            .As<IMongoClient>();

        services.AddSingletonAs(c => GetDatabase(c, mongoDatabaseName))
            .As<IMongoDatabase>();

        services.AddMongoAssetKeyValueStore();

        services.AddYDotNet()
            .AddMongoStorage(options =>
            {
                options.DatabaseName = mongoDatabaseName;
            });

        services.AddAI()
            .AddMongoChatStore(config, options =>
            {
                options.CollectionName = "Chat";
            });

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseMongoDb().SetTokensCollectionName("Identity_Tokens");
            });

        services.AddMessaging()
            .AddMongoDataStore(config);

        services.AddSingleton(typeof(ISnapshotStore<>), typeof(MongoSnapshotStore<>));
        services.AddMigrations(contentDatabase);

        services.AddSingletonAs<MongoDistributedCache>()
            .As<IDistributedCache>();

        services.AddHealthChecks()
            .AddCheck<MongoHealthCheck>("MongoDB", tags: ["node"]);

        services.AddSingletonAs<MongoAppRepository>()
            .As<IAppRepository>().As<ISnapshotStore<App>>().As<IDeleter>();

        services.AddSingletonAs<MongoAssetFolderRepository>()
            .As<IAssetFolderRepository>().As<ISnapshotStore<AssetFolder>>().As<IDeleter>();

        services.AddSingletonAs<MongoHistoryEventRepository>()
            .As<IHistoryEventRepository>().As<IDeleter>();

        services.AddSingletonAs<MongoRequestLogRepository>()
            .As<IRequestLogRepository>();

        services.AddSingletonAs<MongoRoleStore>()
            .As<IRoleStore<IdentityRole>>();

        services.AddSingletonAs<MongoRuleRepository>()
            .As<IRuleRepository>().As<ISnapshotStore<Rule>>().As<IDeleter>();

        services.AddSingletonAs<MongoSchemaRepository>()
            .As<ISchemaRepository>().As<ISnapshotStore<Schema>>().As<IDeleter>();

        services.AddSingletonAs<MongoSchemasHash>()
            .AsOptional<ISchemasHash>().As<IEventConsumer>().As<IDeleter>();

        services.AddSingletonAs<MongoTeamRepository>()
            .As<ITeamRepository>().As<ISnapshotStore<Team>>();

        services.AddSingletonAs<MongoTextIndexerState>()
            .As<ITextIndexerState>().As<IDeleter>();

        services.AddSingletonAs<MongoTokenStoreInitializer>()
            .AsSelf();

        services.AddSingletonAs<MongoUsageRepository>()
            .As<IUsageRepository>();

        services.AddSingletonAs<MongoUserStore>()
            .As<IUserStore<IdentityUser>>().As<IUserFactory>();

        services.AddFlows<FlowEventContext>(config)
            .AddMongoFlowStore<FlowEventContext>();

        services.AddSingletonAs(c =>
        {
            return new MongoShardedAssetRepository(GetSharding(config, "store:mongoDB:assetShardCount"),
                shardKey => ActivatorUtilities.CreateInstance<MongoAssetRepository>(c, shardKey));
        }).As<IAssetRepository>().As<ISnapshotStore<Asset>>().As<IDeleter>();

        services.AddSingletonAs(c =>
        {
            var contentDatabase = GetDatabase(c, mongoContentDatabaseName);

            return new MongoShardedContentRepository(GetSharding(config, "store:mongoDB:contentShardCount"),
                shardKey => ActivatorUtilities.CreateInstance<MongoContentRepository>(c, shardKey, contentDatabase));
        }).As<IContentRepository>().As<ISnapshotStore<WriteContent>>().As<IDeleter>();

        var atlasOptions = config.GetSection("store:mongoDb:atlas").Get<AtlasOptions>() ?? new ();

        if (atlasOptions.IsConfigured() && atlasOptions.FullTextEnabled)
        {
            services.Configure<AtlasOptions>(config.GetSection("store:mongoDb:atlas"));

            services.AddHttpClient("Atlas", options =>
            {
                options.BaseAddress = new Uri("https://cloud.mongodb.com/");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    Credentials = new NetworkCredential(atlasOptions.PublicKey, atlasOptions.PrivateKey, "cloud.mongodb.com"),
                };
            });

            services.AddSingletonAs(c =>
            {
                return new MongoShardedTextIndex<Dictionary<string, string>>(GetSharding(config, "store:mongoDB:textShardCount"),
                    shardKey => ActivatorUtilities.CreateInstance<AtlasTextIndex>(c, shardKey));
            }).AsOptional<ITextIndex>().As<IDeleter>();
        }
        else
        {
            services.AddSingletonAs(c =>
            {
                return new MongoShardedTextIndex<List<MongoTextIndexEntityText>>(GetSharding(config, "store:mongoDB:textShardCount"),
                    shardKey => ActivatorUtilities.CreateInstance<MongoTextIndex>(c, shardKey));
            }).AsOptional<ITextIndex>().As<IDeleter>();
        }

        services.AddInitializer<JsonSerializerOptions>("Serializer (BSON)", jsonSerializerOptions =>
        {
            var representation = config.GetValue<BsonType>("store:mongoDB:valueRepresentation");

            BsonJsonConvention.Register(jsonSerializerOptions, representation);
        }, int.MinValue);
    }

    private static void AddMigrations(this IServiceCollection services, Func<IServiceProvider, IMongoDatabase> contentDatabase)
    {
        services.AddSingletonAs<MongoMigrationStatus>()
            .As<IMigrationStatus>();

        services.AddTransientAs<ConvertOldSnapshotStores>()
            .As<IMigration>();

        services.AddTransientAs<CopyRuleStatistics>()
            .As<IMigration>();

        services.AddTransientAs(c => new DeleteContentCollections(contentDatabase(c)))
            .As<IMigration>();

        services.AddTransientAs(c => new RestructureContentCollection(contentDatabase(c)))
            .As<IMigration>();

        services.AddTransientAs(c => new ConvertDocumentIds(c.GetRequiredService<IMongoDatabase>(), contentDatabase(c)))
            .As<IMigration>();

        services.AddTransientAs<ConvertRuleEventsJson>()
            .As<IMigration>();

        services.AddTransientAs<RenameAssetSlugField>()
            .As<IMigration>();

        services.AddTransientAs<RenameAssetMetadata>()
            .As<IMigration>();

        services.AddTransientAs<AddAppIdToEventStream>()
            .As<IMigration>();

        services.AddTransientAs<ConvertBackup>()
            .As<IMigration>();
    }

    private static IMongoClient GetMongoClient(string configuration)
    {
        return Singletons<IMongoClient>.GetOrAdd(configuration, connectionString =>
        {
            return MongoClientFactory.Create(connectionString, settings =>
            {
                settings.ClusterConfigurator = builder =>
                {
                    builder.Subscribe(new DiagnosticsActivityEventSubscriber());
                };
            });
        });
    }

    private static IShardingStrategy GetSharding(IConfiguration config, string name)
    {
        var shardCount = config.GetValue<int>(name);

        return shardCount > 0 && shardCount <= 100 ? new PartitionedSharding(shardCount) : SingleSharding.Instance;
    }

    private static IMongoDatabase GetDatabase(IServiceProvider serviceProvider, string name)
    {
        return serviceProvider.GetRequiredService<IMongoClient>().GetDatabase(name);
    }
}
