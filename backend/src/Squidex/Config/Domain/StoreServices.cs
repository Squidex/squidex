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
using Migrations.Migrations.MongoDb;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
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
using Squidex.Infrastructure.Log;
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

                    services.AddSingletonAs(c => GetClient(mongoConfiguration))
                        .As<IMongoClient>();

                    services.AddSingletonAs(c => GetDatabase(c, mongoDatabaseName))
                        .As<IMongoDatabase>();

                    services.AddTransientAs(c => new DeleteContentCollections(GetDatabase(c, mongoContentDatabaseName)))
                        .As<IMigration>();

                    services.AddTransientAs(c => new RestructureContentCollection(GetDatabase(c, mongoContentDatabaseName)))
                        .As<IMigration>();

                    services.AddSingletonAs<MongoMigrationStatus>()
                        .As<IMigrationStatus>();

                    services.AddTransientAs<ConvertOldSnapshotStores>()
                        .As<IMigration>();

                    services.AddTransientAs(c => new ConvertDocumentIds(GetDatabase(c, mongoDatabaseName), GetDatabase(c, mongoContentDatabaseName)))
                        .As<IMigration>();

                    services.AddTransientAs<ConvertRuleEventsJson>()
                        .As<IMigration>();

                    services.AddTransientAs<RenameAssetSlugField>()
                        .As<IMigration>();

                    services.AddTransientAs<RenameAssetMetadata>()
                        .As<IMigration>();

                    services.AddTransientAs<AddAppIdToEventStream>()
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
                        .As<IAssetRepository>().As<ISnapshotStore<AssetState, DomainId>>();

                    services.AddSingletonAs<MongoAssetFolderRepository>()
                        .As<IAssetFolderRepository>().As<ISnapshotStore<AssetFolderState, DomainId>>();

                    services.AddSingletonAs(c => ActivatorUtilities.CreateInstance<MongoContentRepository>(c, GetDatabase(c, mongoContentDatabaseName)))
                        .As<IContentRepository>().As<ISnapshotStore<ContentState, DomainId>>();

                    services.AddSingletonAs<MongoTextIndex>()
                        .AsOptional<ITextIndex>();

                    services.AddSingletonAs<MongoTextIndexerState>()
                        .As<ITextIndexerState>();

                    var registration = services.FirstOrDefault(x => x.ServiceType == typeof(IPersistedGrantStore));

                    if (registration == null || registration.ImplementationType == typeof(InMemoryPersistedGrantStore))
                    {
                        services.AddSingletonAs<MongoPersistedGrantStore>()
                            .As<IPersistedGrantStore>();
                    }
                }
            });

            services.AddSingleton(typeof(IStore<>), typeof(Store<>));
        }

        private static IMongoClient GetClient(string configuration)
        {
            return Singletons<IMongoClient>.GetOrAdd(configuration, s => new MongoClient(s));
        }

        private static IMongoDatabase GetDatabase(IServiceProvider service, string name)
        {
            return service.GetRequiredService<IMongoClient>().GetDatabase(name);
        }
    }
}
