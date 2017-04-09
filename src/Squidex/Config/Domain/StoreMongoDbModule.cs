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
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Apps.Services.Implementations;
using Squidex.Read.Assets.Repositories;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.History.Repositories;
using Squidex.Read.MongoDb.Apps;
using Squidex.Read.MongoDb.Assets;
using Squidex.Read.MongoDb.Contents;
using Squidex.Read.MongoDb.History;
using Squidex.Read.MongoDb.Infrastructure;
using Squidex.Read.MongoDb.Schemas;
using Squidex.Read.MongoDb.Users;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Schemas.Services.Implementations;
using Squidex.Read.Users.Repositories;

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

            var contentDatabase = Configuration.GetValue<string>("store:mongoDb:databaseNameContent");

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

            builder.Register<IUserStore<IdentityUser>>(c =>
                {
                    var usersCollection = c.ResolveNamed<IMongoDatabase>(MongoDatabaseRegistration).GetCollection<IdentityUser>("Identity_Users");

                    IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
                    IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);

                    return new UserStore<IdentityUser>(usersCollection);
                })
                .SingleInstance();

            builder.Register<IRoleStore<IdentityRole>>(c =>
                {
                    var rolesCollection = c.ResolveNamed<IMongoDatabase>(MongoDatabaseRegistration).GetCollection<IdentityRole>("Identity_Roles");

                    IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);

                    return new RoleStore<IdentityRole>(rolesCollection);
                })
                .SingleInstance();

            builder.RegisterType<MongoUserRepository>()
                .As<IUserRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MongoPersistedGrantStore>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IPersistedGrantStore>()
                .SingleInstance();

            builder.RegisterType<MongoEventConsumerInfoRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IEventConsumerInfoRepository>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoContentRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoContentDatabaseRegistration))
                .As<IContentRepository>()
                .As<IEventConsumer>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoAppRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IAppRepository>()
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

            builder.RegisterType<MongoHistoryEventRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<IHistoryEventRepository>()
                .As<IEventConsumer>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoSchemaRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                .As<ISchemaRepository>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.Register(c =>
                new CompoundEventConsumer(
                    c.Resolve<MongoSchemaRepository>(), 
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
