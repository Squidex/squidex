// ==========================================================================
//  MongoDbModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.EventStore;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.History.Repositories;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Users.Repositories;
using Squidex.Store.MongoDb.Apps;
using Squidex.Store.MongoDb.History;
using Squidex.Store.MongoDb.Infrastructure;
using Squidex.Store.MongoDb.Schemas;
using Squidex.Store.MongoDb.Users;

namespace Squidex.Store.MongoDb
{
    public class MongoDbModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<MyMongoDbOptions>>().Value;

                var mongoDbClient = new MongoClient(options.ConnectionString);
                var mongoDatabase = mongoDbClient.GetDatabase(options.DatabaseName);

                return mongoDatabase;
            }).SingleInstance();

            builder.Register<IUserStore<IdentityUser>>(context =>
            {
                var usersCollection = context.Resolve<IMongoDatabase>().GetCollection<IdentityUser>("Identity_Users");

                IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
                IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);

                return new UserStore<IdentityUser>(usersCollection);
            }).SingleInstance();

            builder.Register<IRoleStore<IdentityRole>>(context =>
            {
                var rolesCollection = context.Resolve<IMongoDatabase>().GetCollection<IdentityRole>("Identity_Roles");

                IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);

                return new RoleStore<IdentityRole>(rolesCollection);
            }).SingleInstance();

            builder.RegisterType<MongoPersistedGrantStore>()
                .As<IPersistedGrantStore>()
                .SingleInstance();

            builder.RegisterType<MongoStreamPositionStorage>()
                .As<IStreamPositionStorage>()
                .SingleInstance();

            builder.RegisterType<MongoHistoryEventRepository>()
                .As<IHistoryEventRepository>()
                .SingleInstance();

            builder.RegisterType<MongoUserRepository>()
                .As<IUserRepository>()
                .SingleInstance();

            builder.RegisterType<MongoSchemaRepository>()
                .As<ISchemaRepository>()
                .As<ICatchEventConsumer>()
                .SingleInstance();

            builder.RegisterType<MongoAppRepository>()
                .As<IAppRepository>()
                .As<ICatchEventConsumer>()
                .SingleInstance();
        }
    }
}
