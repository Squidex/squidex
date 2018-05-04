// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Migrate_01.Migrations;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.History;
using Squidex.Domain.Apps.Entities.MongoDb.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Domain.Users.MongoDb.Infrastructure;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared.Users;

namespace Squidex.Config.Domain
{
    public static class StoreServices
    {
        public static void AddMyStoreServices(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("store:type", new Options
            {
                ["MongoDB"] = () =>
                {
                    var mongoConfiguration = config.GetRequiredValue("store:mongoDb:configuration");
                    var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database");
                    var mongoContentDatabaseName = config.GetOptionalValue("store:mongoDb:contentDatabase", mongoDatabaseName);

                    var mongoClient = Singletons<IMongoClient>.GetOrAdd(mongoConfiguration, s => new MongoClient(s));
                    var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);
                    var mongoContentDatabase = mongoClient.GetDatabase(mongoContentDatabaseName);

                    services.AddSingleton(typeof(ISnapshotStore<,>), typeof(MongoSnapshotStore<,>));

                    services.AddSingletonAs(mongoDatabase)
                        .As<IMongoDatabase>();

                    services.AddSingletonAs<MongoXmlRepository>()
                        .As<IXmlRepository>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoMigrationStatus>()
                        .As<IMigrationStatus>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoPersistedGrantStore>()
                        .As<IPersistedGrantStore>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoUsageStore>()
                        .As<IUsageStore>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoRuleEventRepository>()
                        .As<IRuleEventRepository>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoUserStore>()
                        .As<IUserStore<IUser>>()
                        .As<IUserFactory>()
                        .As<IUserResolver>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoRoleStore>()
                        .As<IRoleStore<IRole>>()
                        .As<IRoleFactory>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoHistoryEventRepository>()
                        .As<IHistoryEventRepository>()
                        .As<IEventConsumer>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoAssetStatsRepository>()
                        .As<IAssetStatsRepository>()
                        .As<IEventConsumer>()
                        .As<IInitializable>();

                    services.AddSingletonAs<MongoAssetRepository>()
                        .As<IAssetRepository>()
                        .As<ISnapshotStore<AssetState, Guid>>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoContentRepository(mongoContentDatabase, c.GetService<IAppProvider>()))
                        .As<IContentRepository>()
                        .As<ISnapshotStore<ContentState, Guid>>()
                        .As<IEventConsumer>()
                        .As<IInitializable>();

                    services.AddTransientAs(c => new DeleteArchiveCollectionSetup(mongoContentDatabase))
                        .As<IMigration>();

                    services.AddTransientAs<ConvertOldSnapshotStores>()
                        .As<IMigration>();
                }
            });

            services.AddSingleton(typeof(IStore<>), typeof(Store<>));
        }
    }
}
