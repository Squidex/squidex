// ==========================================================================
//  InfrastructureModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Core.Schemas;
using Squidex.Core.Schemas.Json;
using Squidex.Infrastructure.CQRS.Autofac;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.EventStore;
using Squidex.Store.MongoDb.Infrastructure;

namespace Squidex.Config.Domain
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterType<ActionContextAccessor>()
                .As<IActionContextAccessor>()
                .SingleInstance();

            builder.RegisterType<MongoStreamPositionStorage>()
                .As<IStreamPositionStorage>()
                .SingleInstance();

            builder.RegisterType<AutofacDomainObjectFactory>()
                .As<IDomainObjectFactory>()
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

            builder.RegisterType<SchemaJsonSerializer>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FieldRegistry>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
