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
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.Edm;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.History;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Read.Schemas.Services.Implementations;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Pipeline;

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

            builder.Register(c => new GraphQLUrlGenerator(
                    c.Resolve<IOptions<MyUrlsOptions>>(),
                    c.Resolve<IAssetStore>(),
                    Configuration.GetValue<bool>("assetStore:exposeSourceUrl")))
                .As<IGraphQLUrlGenerator>()
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

            builder.RegisterType<CachingSchemaProvider>()
                .As<ISchemaProvider>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AssetUserPictureStore>()
                .As<IUserPictureStore>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ContentHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<NoopAppPlanBillingManager>()
                .As<IAppPlanBillingManager>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<CachingGraphQLService>()
                .As<IGraphQLService>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<WebhookDequeuer>()
                .As<IExternalSystem>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<WebhookEnqueuer>()
                .As<IEventConsumer>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<WebhookSender>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<EdmModelBuilder>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
