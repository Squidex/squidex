// ==========================================================================
//  InfrastructureDependencies.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Net;
using Autofac;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using PinkParrot.Infrastructure.CQRS.Autofac;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.CQRS.EventStore;

namespace PinkParrot.Configurations
{
    public class InfrastructureDependencies : Module
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

            eventStore.ConnectAsync().Wait();

            builder.RegisterInstance(new UserCredentials("admin", "changeit"))
                .SingleInstance();

            builder.RegisterType<AutofacDomainObjectFactory>()
                .As<IDomainObjectFactory>()
                .SingleInstance();

            builder.RegisterType<DefaultNameResolver>()
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
        }
    }
}
