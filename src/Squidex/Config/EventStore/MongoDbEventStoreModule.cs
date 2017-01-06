// ==========================================================================
//  MongoDbEventStoreModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb.EventStore;

namespace Squidex.Config.EventStore
{
    public class MongoDbEventStoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MongoEventStore>()
                .As<IEventStore>()
                .SingleInstance();

            builder.RegisterType<DefaultNameResolver>()
                .As<IStreamNameResolver>()
                .SingleInstance();
        }
    }
}
