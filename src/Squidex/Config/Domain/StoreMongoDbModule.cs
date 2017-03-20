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
using Squidex.Read.Contents.Repositories;
using Squidex.Read.History.Repositories;
using Squidex.Read.MongoDb.Apps;
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
        private const string MongoDatabaseName = "MongoDatabaseName";
        private const string MongoDatabaseNameContent = "MongoDatabaseNameContent";

        private IConfiguration Configuration { get; }

        public StoreMongoDbModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var databaseName = Configuration.GetValue<string>("squidex:stores:mongoDb:databaseName");

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ConfigurationException("You must specify the MongoDB database name in the 'squidex:stores:mongoDb:databaseName' configuration section.");
            }

            var connectionString = Configuration.GetValue<string>("squidex:stores:mongoDb:connectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationException("You must specify the MongoDB connection string in the 'squidex:stores:mongoDb:connectionString' configuration section.");
            }

            var databaseNameContent = Configuration.GetValue<string>("squidex:stores:mongoDb:databaseNameContent");

            if (string.IsNullOrWhiteSpace(databaseNameContent))
            {
                databaseNameContent = databaseName;
            }

            builder.Register(c => new MongoClient(connectionString))
                .As<IMongoClient>()
                .SingleInstance();

            builder.Register(c => c.Resolve<IMongoClient>().GetDatabase(databaseName))
                .Named<IMongoDatabase>(MongoDatabaseName)
                .SingleInstance();

            builder.Register(c => c.Resolve<IMongoClient>().GetDatabase(databaseNameContent))
                .Named<IMongoDatabase>(MongoDatabaseNameContent)
                .SingleInstance();

            builder.Register<IUserStore<IdentityUser>>(c =>
                {
                    var usersCollection = c.ResolveNamed<IMongoDatabase>(MongoDatabaseName).GetCollection<IdentityUser>("Identity_Users");

                    IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
                    IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);

                    return new UserStore<IdentityUser>(usersCollection);
                })
                .SingleInstance();

            builder.Register<IRoleStore<IdentityRole>>(c =>
                {
                    var rolesCollection = c.ResolveNamed<IMongoDatabase>(MongoDatabaseName).GetCollection<IdentityRole>("Identity_Roles");

                    IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);

                    return new RoleStore<IdentityRole>(rolesCollection);
                })
                .SingleInstance();

            builder.RegisterType<MongoUserRepository>()
                .As<IUserRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MongoPersistedGrantStore>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IPersistedGrantStore>()
                .SingleInstance();

            builder.RegisterType<MongoEventConsumerInfoRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IEventConsumerInfoRepository>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoContentRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseNameContent))
                .As<IContentRepository>()
                .As<IEventConsumer>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoHistoryEventRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IHistoryEventRepository>()
                .As<IEventConsumer>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoSchemaRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<ISchemaRepository>()
                .As<IExternalSystem>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MongoAppRepository>()
                .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseName))
                .As<IAppRepository>()
                .As<IEventConsumer>()
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
