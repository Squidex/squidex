// ==========================================================================
//  MongoDbModule.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PinkParrot.Infrastructure.CQRS.EventStore;
using PinkParrot.Store.MongoDb.Infrastructure;

namespace PinkParrot.Store.MongoDb
{
    public class MongoDbModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<MongoDbOptions>>().Value;

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

            builder.RegisterType<MongoPositionStorage>()
                .As<IStreamPositionStorage>()
                .SingleInstance();
        }
    }
}
