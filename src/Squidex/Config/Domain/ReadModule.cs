// ==========================================================================
//  ReadModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Services;
using Squidex.Read.Apps.Services.Implementations;
using Squidex.Read.Contents;
using Squidex.Read.Contents.Builders;
using Squidex.Read.History;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Services;
using Squidex.Read.Schemas.Services.Implementations;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Squidex.Config.Domain
{
    public sealed class ReadModule : Module
    {
        private IConfiguration Configuration { get; }

        public ReadModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => c.Resolve<IOptions<MyUsageOptions>>().Value?.Plans ?? Enumerable.Empty<ConfigAppLimitsPlan>())
                .As<IEnumerable<ConfigAppLimitsPlan>>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<CachingAppProvider>()
                .As<IAppProvider>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ConfigAppPlansProvider>()
                .As<IAppPlansProvider>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<NoopAppPlanBillingManager>()
                .As<IAppPlanBillingManager>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<CachingSchemaProvider>()
                .As<ISchemaProvider>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();

            builder.RegisterType<ContentHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();

            builder.RegisterType<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .SingleInstance();

            builder.RegisterType<WebhookInvoker>()
                .As<IEventConsumer>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<EdmModelBuilder>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
