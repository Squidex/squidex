// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Migrate_01.Migrations.MongoDb;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.FullText;
using Squidex.Domain.Apps.Entities.MongoDb.History;
using Squidex.Domain.Apps.Entities.MongoDb.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Domain.Users.MongoDb.Infrastructure;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Log.Store;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Config.Domain
{
    public static class StoreServices
    {
        public static void AddSquidexStoreServices(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("store:type", new Alternatives
            {
                ["MongoDB"] = () =>
                {
                    var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                    var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");
                    var mongoContentDatabaseName = config.GetOptionalValue("store:mongoDb:contentDatabase", mongoDatabaseName);

                    services.AddSingleton(typeof(ISnapshotStore<,>), typeof(MongoSnapshotStore<,>));

                    services.AddSingletonAs(_ => Singletons<IMongoClient>.GetOrAdd(mongoConfiguration, s => new MongoClient(s)))
                        .As<IMongoClient>();

                    services.AddSingletonAs(c => c.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName))
                        .As<IMongoDatabase>();

                    services.AddTransientAs(c => new DeleteContentCollections(c.GetRequiredService<IMongoClient>().GetDatabase(mongoContentDatabaseName)))
                        .As<IMigration>();

                    services.AddTransientAs(c => new RestructureContentCollection(c.GetRequiredService<IMongoClient>().GetDatabase(mongoContentDatabaseName)))
                        .As<IMigration>();

                    services.AddSingletonAs<MongoMigrationStatus>()
                        .As<IMigrationStatus>();

                    services.AddTransientAs<ConvertOldSnapshotStores>()
                        .As<IMigration>();

                    services.AddTransientAs<ConvertRuleEventsJson>()
                        .As<IMigration>();

                    services.AddTransientAs<RenameAssetSlugField>()
                        .As<IMigration>();

                    services.AddTransientAs<RenameAssetMetadata>()
                        .As<IMigration>();

                    services.AddHealthChecks()
                        .AddCheck<MongoDBHealthCheck>("MongoDB", tags: new[] { "node" });

                    services.AddSingletonAs<MongoRequestLogRepository>()
                        .As<IRequestLogRepository>();

                    services.AddSingletonAs<MongoUsageRepository>()
                        .As<IUsageRepository>();

                    services.AddSingletonAs<MongoRuleEventRepository>()
                        .As<IRuleEventRepository>();

                    services.AddSingletonAs<MongoHistoryEventRepository>()
                        .As<IHistoryEventRepository>();

                    services.AddSingletonAs<MongoRoleStore>()
                        .As<IRoleStore<IdentityRole>>();

                    services.AddSingletonAs<MongoUserStore>()
                        .As<IUserStore<IdentityUser>>().As<IUserFactory>();

                    services.AddSingletonAs<MongoAssetRepository>()
                        .As<IAssetRepository>().As<ISnapshotStore<AssetState, Guid>>();

                    services.AddSingletonAs<MongoAssetFolderRepository>()
                        .As<IAssetFolderRepository>().As<ISnapshotStore<AssetFolderState, Guid>>();

                    services.AddSingletonAs(c => new MongoContentRepository(
                            c.GetRequiredService<IMongoClient>().GetDatabase(mongoContentDatabaseName),
                            c.GetRequiredService<IAppProvider>(),
                            c.GetRequiredService<ITextIndexer>(),
                            c.GetRequiredService<IJsonSerializer>()))
                        .As<IContentRepository>().As<ISnapshotStore<ContentState, Guid>>();

                    var registration = services.FirstOrDefault(x => x.ServiceType == typeof(IPersistedGrantStore));

                    if (registration == null || registration.ImplementationType == typeof(InMemoryPersistedGrantStore))
                    {
                        services.AddSingletonAs<MongoPersistedGrantStore>()
                            .As<IPersistedGrantStore>();
                    }

                    services.AddSingletonAs(c =>
                    {
                        var database = c.GetRequiredService<IMongoDatabase>();

                        var mongoBucket = new GridFSBucket<string>(database, new GridFSBucketOptions
                        {
                            BucketName = "fullText"
                        });

                        return new MongoIndexStorage(mongoBucket);
                    }).As<IIndexStorage>();
                }
            });

            services.AddSingleton(typeof(IStore<>), typeof(Store<>));
        }
    }
}
