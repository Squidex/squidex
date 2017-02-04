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
using Microsoft.Extensions.Configuration;
using Squidex.Core.Schemas;
using Squidex.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Replay;

namespace Squidex.Config.Domain
{
    public class InfrastructureModule : Module
    {
        public IConfiguration Configuration { get; }

        public InfrastructureModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterType<ActionContextAccessor>()
                .As<IActionContextAccessor>()
                .SingleInstance();

            builder.RegisterType<DefaultDomainObjectRepository>()
                .As<IDomainObjectRepository>()
                .SingleInstance();

            builder.RegisterType<AggregateHandler>()
                .As<IAggregateHandler>()
                .SingleInstance();

            builder.RegisterType<InMemoryCommandBus>()
                .As<ICommandBus>()
                .SingleInstance();

            builder.RegisterType<DefaultNameResolver>()
                .As<IStreamNameResolver>()
                .SingleInstance();

            builder.RegisterType<ReplayGenerator>()
                .As<ICliCommand>()
                .SingleInstance();

            builder.RegisterType<EventDataFormatter>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<EventReceiver>()
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
