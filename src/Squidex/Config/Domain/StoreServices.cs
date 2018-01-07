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
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Apps;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.History;
using Squidex.Domain.Apps.Entities.MongoDb.Rules;
using Squidex.Domain.Apps.Entities.MongoDb.Schemas;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Domain.Users.MongoDb.Infrastructure;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Grains;
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

                    services.AddSingletonAs(c => new MongoXmlRepository(mongoDatabase))
                        .As<IXmlRepository>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoMigrationStatus(mongoDatabase))
                        .As<IMigrationStatus>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoSnapshotStore<EventConsumerState, string>(mongoDatabase, c.GetRequiredService<JsonSerializer>()))
                        .As<ISnapshotStore<EventConsumerState, string>>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoUserStore(mongoDatabase))
                        .As<IUserStore<IUser>>()
                        .As<IUserFactory>()
                        .As<IUserResolver>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoRoleStore(mongoDatabase))
                        .As<IRoleStore<IRole>>()
                        .As<IRoleFactory>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoPersistedGrantStore(mongoDatabase))
                        .As<IPersistedGrantStore>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoUsageStore(mongoDatabase))
                        .As<IUsageStore>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoRuleEventRepository(mongoDatabase))
                        .As<IRuleEventRepository>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoAppRepository(mongoDatabase))
                        .As<IAppRepository>()
                        .As<ISnapshotStore<AppState, Guid>>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoAssetRepository(mongoDatabase))
                        .As<IAssetRepository>()
                        .As<ISnapshotStore<AssetState, Guid>>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoRuleRepository(mongoDatabase))
                        .As<IRuleRepository>()
                        .As<ISnapshotStore<RuleState, Guid>>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoSchemaRepository(mongoDatabase))
                        .As<ISchemaRepository>()
                        .As<ISnapshotStore<SchemaState, Guid>>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoContentRepository(mongoContentDatabase, c.GetService<IAppProvider>()))
                        .As<IContentRepository>()
                        .As<ISnapshotStore<ContentState, Guid>>()
                        .As<IEventConsumer>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoHistoryEventRepository(mongoDatabase, c.GetServices<IHistoryEventsCreator>()))
                        .As<IHistoryEventRepository>()
                        .As<IEventConsumer>()
                        .As<IInitializable>();

                    services.AddSingletonAs(c => new MongoAssetStatsRepository(mongoDatabase))
                        .As<IAssetStatsRepository>()
                        .As<IEventConsumer>()
                        .As<IInitializable>();
                }
            });
        }
    }
}
