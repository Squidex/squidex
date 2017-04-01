// ==========================================================================
//  InfrastructureModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Core.Schemas;
using Squidex.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Log;
using IntrospectionExtensions = System.Reflection.IntrospectionExtensions;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Squidex.Config.Domain
{
    public class InfrastructureModule : Module
    {
        private IConfiguration Configuration { get; }

        public InfrastructureModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (Configuration.GetValue<bool>("squidex:logging:human"))
            {
                builder.Register(c => new Func<IObjectWriter>(() => new JsonLogWriter(Formatting.Indented, true)))
                    .AsSelf()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new Func<IObjectWriter>(() => new JsonLogWriter()))
                    .AsSelf()
                    .SingleInstance();
            }

            builder.Register(c => new ApplicationInfoLogAppender(IntrospectionExtensions.GetTypeInfo(typeof(InfrastructureModule)).Assembly))
                .As<ILogAppender>()
                .SingleInstance();

            builder.Register(c => new TimestampLogAppender())
                .As<ILogAppender>()
                .SingleInstance();

            builder.RegisterType<DebugLogChannel>()
                .As<ILogChannel>()
                .SingleInstance();

            builder.RegisterType<ConsoleLogChannel>()
                .As<ILogChannel>()
                .SingleInstance();

            builder.RegisterType<SemanticLog>()
                .As<ISemanticLog>()
                .SingleInstance();

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

            builder.RegisterType<DefaultEventNotifier>()
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
