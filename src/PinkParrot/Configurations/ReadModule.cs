// ==========================================================================
//  ReadDependencies.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Read.Repositories;
using PinkParrot.Read.Repositories.Implementations;
using PinkParrot.Read.Services;
using PinkParrot.Read.Services.Implementations;

namespace PinkParrot.Configurations
{
    public sealed class ReadModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TenantProvider>()
                .As<ITenantProvider>()
                .SingleInstance();

            builder.RegisterType<ModelSchemaProvider>()
                .As<IModelSchemaProvider>()
                .As<ILiveEventConsumer>()
                .SingleInstance();

            builder.RegisterType<MongoModelSchemaRepository>()
                .As<IModelSchemaRepository>()
                .As<ICatchEventConsumer>()
                .SingleInstance();
        }
    }
}
