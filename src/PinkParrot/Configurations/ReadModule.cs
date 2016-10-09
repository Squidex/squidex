// ==========================================================================
//  ReadModule.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Read.Infrastructure.Services;
using PinkParrot.Read.Infrastructure.Services.Implementations;
using PinkParrot.Read.Schemas.Repositories;
using PinkParrot.Read.Schemas.Services;
using PinkParrot.Read.Schemas.Services.Implementations;
using PinkParrot.Store.MongoDb.Schemas;

namespace PinkParrot.Configurations
{
    public sealed class ReadModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TenantProvider>()
                .As<ITenantProvider>()
                .SingleInstance();

            builder.RegisterType<CachingSchemaProvider>()
                .As<ISchemaProvider>()
                .As<ILiveEventConsumer>()
                .SingleInstance();

            builder.RegisterType<MongoSchemaRepository>()
                .As<ISchemaRepository>()
                .As<ICatchEventConsumer>()
                .SingleInstance();
        }
    }
}
