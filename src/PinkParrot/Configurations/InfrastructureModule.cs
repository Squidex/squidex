// ==========================================================================
//  InfrastructureModule.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Net;
using Autofac;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure.CQRS.Autofac;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.CQRS.EventStore;
using PinkParrot.Pipeline;
using PinkParrot.Read.Repositories.Implementations.Mongo;

namespace PinkParrot.Configurations
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var eventStore = 
                EventStoreConnection.Create(
                   ConnectionSettings.Create()
                       .UseConsoleLogger()
                       .UseDebugLogger()
                       .KeepReconnecting()
                       .KeepRetrying(),
                   new IPEndPoint(IPAddress.Loopback, 1113));

            var mongoDbClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoDbClient.GetDatabase("PinkParrot");

            eventStore.ConnectAsync().Wait();
            
            builder.RegisterInstance(new UserCredentials("admin", "changeit"))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterType<ActionContextAccessor>()
                .As<IActionContextAccessor>()
                .SingleInstance();

            builder.RegisterInstance(mongoDatabase)
                .As<IMongoDatabase>()
                .SingleInstance();

            builder.RegisterType<MongoPositionStorage>()
                .As<IStreamPositionStorage>()
                .SingleInstance();

            builder.RegisterType<AutofacDomainObjectFactory>()
                .As<IDomainObjectFactory>()
                .SingleInstance();

            builder.RegisterInstance(new DefaultNameResolver("pinkparrot"))
                .As<IStreamNameResolver>()
                .SingleInstance();

            builder.RegisterInstance(eventStore)
                .As<IEventStoreConnection>()
                .SingleInstance();

            builder.RegisterType<EventStoreDomainObjectRepository>()
                .As<IDomainObjectRepository>()
                .SingleInstance();

            builder.RegisterType<InMemoryCommandBus>()
                .As<ICommandBus>()
                .SingleInstance();

            builder.RegisterType<EventStoreBus>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FieldRegistry>()
                .AsSelf()
                .SingleInstance();
        }
    }

    public static class InfrastructureDependencie
    {
        public static void UseAppEventBus(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<EventStoreBus>().Subscribe("pinkparrot");
        }

        public static void UseAppTenants(this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantMiddleware>();
        }
    }
}
