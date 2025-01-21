// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Migrations.Migrations.MongoDb;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Squidex.Config.Domain;

public static class StoreServices
{
    public static void AddSquidexStoreServices(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("store:type", new Alternatives
        {
            ["MongoDB"] = () =>
            {
                var mongoDatabaseName = config.GetRequiredValue("store:mongoDb:database")!;
                var mongoContentDatabaseName = config.GetOptionalValue("store:mongoDb:contentDatabase", mongoDatabaseName)!;

                static IMongoDatabase GetDatabase(IServiceProvider serviceProvider, string name)
                {
                    return serviceProvider.GetRequiredService<IMongoClient>().GetDatabase(name);
                }

                services.AddTransientAs(c => new DeleteContentCollections(GetDatabase(c, mongoContentDatabaseName)))
                    .As<IMigration>();

                services.AddTransientAs(c => new RestructureContentCollection(GetDatabase(c, mongoContentDatabaseName)))
                    .As<IMigration>();

                services.AddTransientAs(c => new ConvertDocumentIds(GetDatabase(c, mongoDatabaseName), GetDatabase(c, mongoContentDatabaseName)))
                    .As<IMigration>();

                services.AddTransientAs<ConvertOldSnapshotStores>()
                    .As<IMigration>();

                services.AddTransientAs<CopyRuleStatistics>()
                    .As<IMigration>();

                services.AddTransientAs<ConvertRuleEventsJson>()
                    .As<IMigration>();

                services.AddTransientAs<RenameAssetSlugField>()
                    .As<IMigration>();

                services.AddTransientAs<RenameAssetMetadata>()
                    .As<IMigration>();

                services.AddTransientAs<AddAppIdToEventStream>()
                    .As<IMigration>();

                services.AddSquidexMongoStore(config);
            },
        });

        services.AddSingleton(typeof(IStore<>),
            typeof(Store<>));

        services.AddSingleton(typeof(IPersistenceFactory<>),
            typeof(Store<>));
    }
}
