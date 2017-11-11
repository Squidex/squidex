// ==========================================================================
//  StoreServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.History;
using Squidex.Domain.Apps.Read.History.Repositories;
using Squidex.Domain.Apps.Read.MongoDb.Apps;
using Squidex.Domain.Apps.Read.MongoDb.Assets;
using Squidex.Domain.Apps.Read.MongoDb.Contents;
using Squidex.Domain.Apps.Read.MongoDb.History;
using Squidex.Domain.Apps.Read.MongoDb.Rules;
using Squidex.Domain.Apps.Read.MongoDb.Schemas;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Domain.Users.MongoDb.Infrastructure;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
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

                    services.AddSingleton(c => new MongoUserStore(mongoDatabase))
                        .As<IUserStore<IUser>>()
                        .As<IUserFactory>()
                        .As<IUserResolver>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoRoleStore(mongoDatabase))
                        .As<IRoleStore<IRole>>()
                        .As<IRoleFactory>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoPersistedGrantStore(mongoDatabase))
                        .As<IPersistedGrantStore>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoUsageStore(mongoDatabase))
                        .As<IUsageStore>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoEventConsumerInfoRepository(mongoDatabase))
                        .As<IEventConsumerInfoRepository>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoContentRepository(mongoContentDatabase, c.GetService<ISchemaProvider>()))
                        .As<IContentRepository>()
                        .As<IEventConsumer>();

                    services.AddSingleton(c => new MongoRuleEventRepository(mongoDatabase))
                        .As<IRuleEventRepository>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoHistoryEventRepository(mongoDatabase, c.GetServices<IHistoryEventsCreator>()))
                        .As<IHistoryEventRepository>()
                        .As<IEventConsumer>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoAppRepository(mongoDatabase))
                        .As<IAppRepository>()
                        .As<IAppEventConsumer>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoSchemaRepository(mongoDatabase, c.GetRequiredService<FieldRegistry>()))
                        .As<ISchemaRepository>()
                        .As<ISchemaEventConsumer>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoAssetStatsRepository(mongoDatabase))
                        .As<IAssetStatsRepository>()
                        .As<IAssetEventConsumer>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoAssetRepository(mongoDatabase))
                        .As<IAssetRepository>()
                        .As<IAssetEventConsumer>()
                        .As<IExternalSystem>();

                    services.AddSingleton(c => new MongoAssetRepository(mongoDatabase))
                        .As<IRuleRepository>()
                        .As<IEventConsumer>()
                        .As<IExternalSystem>();
                }
            });
        }
    }
}
