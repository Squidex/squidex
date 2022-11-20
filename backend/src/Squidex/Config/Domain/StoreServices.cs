// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Migrations.Migrations.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Apps;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.History;
using Squidex.Domain.Apps.Entities.MongoDb.Rules;
using Squidex.Domain.Apps.Entities.MongoDb.Schemas;
using Squidex.Domain.Apps.Entities.MongoDb.Teams;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Domain.Apps.Entities.Teams.DomainObject;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Domain.Users;
using Squidex.Domain.Users.InMemory;
using Squidex.Domain.Users.MongoDb;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Config.Domain;

public static class StoreServices
{
    public static void AddSquidexStoreServices(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("store:type", new Alternatives
        {
            ["MongoDB"] = () =>
            {
                var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration")!;
                var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database")!;
                var mongoContentDatabaseName = config.GetOptionalValue("store:mongoDb:contentDatabase", mongoDatabaseName)!;

                services.AddSingleton(typeof(ISnapshotStore<>), typeof(MongoSnapshotStore<>));

                services.AddSingletonAs(c => GetMongoClient(mongoConfiguration))
                    .As<IMongoClient>();

                services.AddSingletonAs(c => GetDatabase(c, mongoDatabaseName))
                    .As<IMongoDatabase>();

                services.AddSingletonAs<MongoMigrationStatus>()
                    .As<IMigrationStatus>();

                services.AddTransientAs<ConvertOldSnapshotStores>()
                    .As<IMigration>();

                services.AddTransientAs(c => new DeleteContentCollections(GetDatabase(c, mongoContentDatabaseName)))
                    .As<IMigration>();

                services.AddTransientAs(c => new RestructureContentCollection(GetDatabase(c, mongoContentDatabaseName)))
                    .As<IMigration>();

                services.AddTransientAs(c => new ConvertDocumentIds(GetDatabase(c, mongoDatabaseName), GetDatabase(c, mongoContentDatabaseName)))
                    .As<IMigration>();

                services.AddSingletonAs(c => ActivatorUtilities.CreateInstance<MongoContentRepository>(c, GetDatabase(c, mongoContentDatabaseName)))
                    .As<IContentRepository>().As<ISnapshotStore<ContentDomainObject.State>>().As<IDeleter>();

                services.AddTransientAs<ConvertRuleEventsJson>()
                    .As<IMigration>();

                services.AddTransientAs<RenameAssetSlugField>()
                    .As<IMigration>();

                services.AddTransientAs<RenameAssetMetadata>()
                    .As<IMigration>();

                services.AddTransientAs<AddAppIdToEventStream>()
                    .As<IMigration>();

                services.AddSingletonAs<MongoDistributedCache>()
                    .As<IDistributedCache>();

                services.AddHealthChecks()
                    .AddCheck<MongoHealthCheck>("MongoDB", tags: new[] { "node" });

                services.AddSingletonAs<MongoAssetKeyValueStore<TusMetadata>>()
                    .As<IAssetKeyValueStore<TusMetadata>>();

                services.AddSingletonAs<MongoRequestLogRepository>()
                    .As<IRequestLogRepository>();

                services.AddSingletonAs<MongoUsageRepository>()
                    .As<IUsageRepository>();

                services.AddSingletonAs<MongoRuleEventRepository>()
                    .As<IRuleEventRepository>().As<IDeleter>();

                services.AddSingletonAs<MongoHistoryEventRepository>()
                    .As<IHistoryEventRepository>().As<IDeleter>();

                services.AddSingletonAs<MongoRoleStore>()
                    .As<IRoleStore<IdentityRole>>();

                services.AddSingletonAs<MongoUserStore>()
                    .As<IUserStore<IdentityUser>>().As<IUserFactory>();

                services.AddSingletonAs<MongoAssetRepository>()
                    .As<IAssetRepository>().As<ISnapshotStore<AssetDomainObject.State>>().As<IDeleter>();

                services.AddSingletonAs<MongoAssetFolderRepository>()
                    .As<IAssetFolderRepository>().As<ISnapshotStore<AssetFolderDomainObject.State>>().As<IDeleter>();

                services.AddSingletonAs<MongoAppRepository>()
                    .As<IAppRepository>().As<ISnapshotStore<AppDomainObject.State>>().As<IDeleter>();

                services.AddSingletonAs<MongoTeamRepository>()
                    .As<ITeamRepository>().As<ISnapshotStore<TeamDomainObject.State>>();

                services.AddSingletonAs<MongoRuleRepository>()
                    .As<IRuleRepository>().As<ISnapshotStore<RuleDomainObject.State>>().As<IDeleter>();

                services.AddSingletonAs<MongoSchemaRepository>()
                    .As<ISchemaRepository>().As<ISnapshotStore<SchemaDomainObject.State>>().As<IDeleter>();

                services.AddSingletonAs<MongoSchemasHash>()
                    .AsOptional<ISchemasHash>().As<IEventConsumer>().As<IDeleter>();

                services.AddSingletonAs<MongoTextIndexerState>()
                    .As<ITextIndexerState>().As<IDeleter>();

                services.AddOpenIddict()
                    .AddCore(builder =>
                    {
                        builder.UseMongoDb<string>()
                            .SetScopesCollectionName("Identity_Scopes")
                            .SetTokensCollectionName("Identity_Tokens");

                        builder.SetDefaultScopeEntity<ImmutableScope>();
                        builder.SetDefaultApplicationEntity<ImmutableApplication>();
                    });

                var atlasOptions = config.GetSection("store:mongoDb:atlas").Get<AtlasOptions>() ?? new ();

                if (atlasOptions.IsConfigured() && atlasOptions.FullTextEnabled)
                {
                    services.Configure<AtlasOptions>(config.GetSection("store:mongoDb:atlas"));

                    services.AddSingletonAs<AtlasTextIndex>()
                        .AsOptional<ITextIndex>().As<IDeleter>();
                }
                else
                {
                    services.AddSingletonAs<MongoTextIndex>()
                        .AsOptional<ITextIndex>().As<IDeleter>();
                }

                services.AddInitializer<JsonSerializerOptions>("Serializer (BSON)", jsonSerializerOptions =>
                {
                    var representation = config.GetValue<BsonType>("store:mongoDB:valueRepresentation");

                    BsonJsonConvention.Register(jsonSerializerOptions, representation);
                }, int.MinValue);
            }
        });

        services.AddSingleton(typeof(IStore<>),
            typeof(Store<>));

        services.AddSingleton(typeof(IPersistenceFactory<>),
            typeof(Store<>));

        services.AddSingletonAs<IInitializable>(c =>
        {
            var service = c.GetRequiredService<IAssetKeyValueStore<TusMetadata>>();

            return new DelegateInitializer(service.GetType().Name, service.InitializeAsync);
        });
    }

    public static IMongoClient GetMongoClient(string configuration)
    {
        return Singletons<IMongoClient>.GetOrAdd(configuration, connectionString =>
        {
            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

            clientSettings.ClusterConfigurator = builder =>
            {
                builder.Subscribe(new DiagnosticsActivityEventSubscriber());
            };

            return new MongoClient(clientSettings);
        });
    }

    private static IMongoDatabase GetDatabase(IServiceProvider serviceProvider, string name)
    {
        return serviceProvider.GetRequiredService<IMongoClient>().GetDatabase(name);
    }
}
