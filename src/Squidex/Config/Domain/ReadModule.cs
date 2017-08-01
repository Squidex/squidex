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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Squidex.Chargebee;
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
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Pipeline;
using Squidex.Shared.Users;

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

            builder.RegisterType<CachingSchemaProvider>()
                .As<ISchemaProvider>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<GraphQLUrlGenerator>()
                .As<IGraphQLUrlGenerator>()
                .SingleInstance();

            builder.RegisterType<AssetUserPictureStore>()
                .As<IUserPictureStore>()
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

            builder.RegisterType<CachingGraphQLService>()
                .As<IGraphQLService>()
                .AsSelf()
                .InstancePerDependency();

            var chargebeeSiteName = Configuration.GetValue<string>("chargebee:siteName");
            var chargebeeApiKey = Configuration.GetValue<string>("chargebee:apiKey");

            if (string.IsNullOrWhiteSpace(chargebeeSiteName))
            {
                throw new ConfigurationException("Configure Chargebee SiteName type with 'chargebee:siteName'.");
            }
            if (string.IsNullOrWhiteSpace(chargebeeApiKey))
            {
                throw new ConfigurationException("Configure Chargebee ApiKey with 'chargebee:apiKey'.");
            }

            builder.Register(c => new ChargebeeAppPlanBillingManager(c.Resolve<UserManager<IUser>>(), chargebeeSiteName, chargebeeApiKey))
                .As<IAppPlanBillingManager>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
