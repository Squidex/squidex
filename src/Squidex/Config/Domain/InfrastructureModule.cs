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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Core.Schemas;
using Squidex.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;

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
            builder.Register(c => SystemClock.Instance)
                .As<IClock>()
                .SingleInstance();

            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterType<ActionContextAccessor>()
                .As<IActionContextAccessor>()
                .SingleInstance();

            builder.RegisterType<DefaultDomainObjectRepository>()
                .As<IDomainObjectRepository>()
                .SingleInstance();

            builder.RegisterType<DefaultDomainObjectFactory>()
                .As<IDomainObjectFactory>()
                .SingleInstance();

            builder.RegisterType<AggregateHandler>()
                .As<IAggregateHandler>()
                .SingleInstance();

            builder.RegisterType<InMemoryCommandBus>()
                .As<ICommandBus>()
                .SingleInstance();

            builder.RegisterType<DefaultMemoryEventNotifier>()
                .As<IEventNotifier>()
                .SingleInstance();

            builder.RegisterType<DefaultNameResolver>()
                .As<IStreamNameResolver>()
                .SingleInstance();

            builder.RegisterType<EventDataFormatter>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SchemaJsonSerializer>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FieldRegistry>()
                .AsSelf()
                .SingleInstance();

            builder.Register(c => new InvalidatingMemoryCache(new MemoryCache(c.Resolve<IOptions<MemoryCacheOptions>>()), c.Resolve<IPubSub>()))
                .As<IMemoryCache>()
                .SingleInstance();
        }
    }
}
