// ==========================================================================
//  StoreServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using IdentityServer4.Stores;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.History;
using Squidex.Domain.Apps.Read.History.Repositories;
using Squidex.Domain.Apps.Read.MongoDb.Assets;
using Squidex.Domain.Apps.Read.MongoDb.Contents;
using Squidex.Domain.Apps.Read.MongoDb.History;
using Squidex.Domain.Apps.Read.MongoDb.Rules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Domain.Users.MongoDb.Infrastructure;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
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
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoSnapshotStore(mongoDatabase, c.GetRequiredService<JsonSerializer>()))
                        .As<ISnapshotStore>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoUserStore(mongoDatabase))
                        .As<IUserStore<IUser>>()
                        .As<IUserFactory>()
                        .As<IUserResolver>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoRoleStore(mongoDatabase))
                        .As<IRoleStore<IRole>>()
                        .As<IRoleFactory>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoPersistedGrantStore(mongoDatabase))
                        .As<IPersistedGrantStore>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoUsageStore(mongoDatabase))
                        .As<IUsageStore>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoContentRepository(mongoContentDatabase, c.GetService<IAppProvider>()))
                        .As<IContentRepository>()
                        .As<IEventConsumer>();

                    services.AddSingletonAs(c => new MongoRuleEventRepository(mongoDatabase))
                        .As<IRuleEventRepository>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoHistoryEventRepository(mongoDatabase, c.GetServices<IHistoryEventsCreator>()))
                        .As<IHistoryEventRepository>()
                        .As<IEventConsumer>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoAssetRepository(mongoDatabase))
                        .As<IAssetRepository>()
                        .As<IAssetEventConsumer>()
                        .As<IExternalSystem>();

                    services.AddSingletonAs(c => new MongoAssetStatsRepository(mongoDatabase))
                        .As<IAssetStatsRepository>()
                        .As<IAssetEventConsumer>()
                        .As<IExternalSystem>();
                }
            });
        }
    }
}
