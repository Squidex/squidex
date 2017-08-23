// ==========================================================================
//  StoreMongoDbModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Autofac.Core;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.History.Repositories;
using Squidex.Domain.Apps.Read.MongoDb.Apps;
using Squidex.Domain.Apps.Read.MongoDb.Assets;
using Squidex.Domain.Apps.Read.MongoDb.Contents;
using Squidex.Domain.Apps.Read.MongoDb.History;
using Squidex.Domain.Apps.Read.MongoDb.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Services.Implementations;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Domain.Users.MongoDb.Infrastructure;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared.Users;

namespace Squidex.Config.Domain
{
    public class StoreMongoDbModule : Module
    {
        private const string MongoClientRegistration = "StoreMongoClient";
        private const string MongoDatabaseRegistration = "StoreMongoDatabaseName";
        private const string MongoContentDatabaseRegistration = "StoreMongoDatabaseNameContent";

        private IConfiguration Configuration { get; }

        public StoreMongoDbModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var configuration = Configuration.GetValue<string>("store:mongoDb:configuration");

            if (string.IsNullOrWhiteSpace(configuration))
            {
                throw new ConfigurationException("Configure the Store MongoDb configuration with 'store:mongoDb:configuration'.");
            }

            var database = Configuration.GetValue<string>("store:mongoDb:database");

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ConfigurationException("Configure the Store MongoDb database with 'store:mongoDb:database'.");
            }

            var contentDatabase = Configuration.GetValue<string>("store:mongoDb:contentDatabase");

            if (string.IsNullOrWhiteSpace(contentDatabase))
            {
                contentDatabase = database;
            }

            builder.Register(c => Singletons<IMongoClient>.GetOrAdd(configuration, s => new MongoClient(s)))
                .Named<IMongoClient>(MongoClientRegistration)
                .SingleInstance();

            builder.Register(c => c.ResolveNamed<IMongoClient>(MongoClientRegistration).GetDatabase(database))
                .Named<IMongoDatabase>(MongoDatabaseRegistration)
                .SingleInstance();

            builder.Register(c => c.ResolveNamed<IMongoClient>(MongoClientRegistration).GetDatabase(contentDatabase))
                .Named<IMongoDatabase>(MongoContentDatabaseRegistration)
                .SingleInstance();

            builder.RegisterType<MongoUserStore>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IUserStore<IUser>>()
                .As<IUserFactory>()
                .As<IUserResolver>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoRoleStore>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IRoleStore<IRole>>()
                .As<IRoleFactory>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoPersistedGrantStore>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IPersistedGrantStore>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoUsageStore>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IUsageStore>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoHistoryEventRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IHistoryEventRepository>()
                .As<IEventConsumer>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoEventConsumerInfoRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IEventConsumerInfoRepository>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoContentRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoContentDatabaseRegistration))
                .As<IContentRepository>()
                .As<IEventConsumer>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoWebhookEventRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IWebhookEventRepository>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoAppRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IAppRepository>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoSchemaRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<ISchemaRepository>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoAssetStatsRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IAssetStatsRepository>()
                .As<IEventConsumer>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoAssetRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IAssetRepository>()
                .As<IEventConsumer>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoSchemaWebhookRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<ISchemaWebhookRepository>()
                .As<IEventConsumer>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();


            builder.Register(c =>
                new CompoundEventConsumer(
                    c.Resolve<MongoSchemaRepository>(),
                    c.Resolve<CachingGraphQLService>(),
                    c.Resolve<CachingSchemaProvider>()))
                .As<IEventConsumer>()
                .AsSelf()
                .SingleInstance();

            builder.Register(c =>
                new CompoundEventConsumer(
                    c.Resolve<MongoAppRepository>(),
                    c.Resolve<CachingAppProvider>()))
                .As<IEventConsumer>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
