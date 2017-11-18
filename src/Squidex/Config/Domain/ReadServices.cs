// ==========================================================================
//  ReadServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.Actions;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.Edm;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.History;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.State.Orleans;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Orleans;
using Squidex.Pipeline;

namespace Squidex.Config.Domain
{
    public static class ReadServices
    {
        public static void AddMyReadServices(this IServiceCollection services, IConfiguration config)
        {
            var exposeSourceUrl = config.GetOptionalValue("assetStore:exposeSourceUrl", true);

            services.AddSingletonAs(c => new GraphQLUrlGenerator(
                    c.GetRequiredService<IOptions<MyUrlsOptions>>(),
                    c.GetRequiredService<IAssetStore>(),
                    exposeSourceUrl))
                .As<IGraphQLUrlGenerator>();

            services.AddSingletonAs(c => c.GetService<IOptions<MyUsageOptions>>()?.Value?.Plans.OrEmpty());

            services.AddSingletonAs<CachingGraphQLService>()
                .As<IGraphQLService>();

            services.AddSingletonAs<ContentQueryService>()
                .As<IContentQueryService>();

            services.AddSingletonAs<ConfigAppPlansProvider>()
                .As<IAppPlansProvider>();

            services.AddSingletonAs<AssetUserPictureStore>()
                .As<IUserPictureStore>();

            services.AddSingletonAs<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<ContentHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingletonAs<NoopAppPlanBillingManager>()
                .As<IAppPlanBillingManager>();

            services.AddSingletonAs<OrleansEventNotifier>()
                .As<IEventNotifier>();

            services.AddSingletonAs<OrleansAppProvider>()
                .As<IAppProvider>();

            services.AddSingletonAs<AppStateEventConsumer>()
                .As<IEventConsumer>();

            services.AddSingletonAs<RuleEnqueuer>()
                .As<IEventConsumer>();

            services.AddSingletonAs<ContentChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<WebhookActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingletonAs<IEventConsumer>(c =>
                new CompoundEventConsumer(c.GetServices<IAssetEventConsumer>().ToArray()));

            services.AddSingletonAs(c =>
            {
                var allEventConsumers = c.GetServices<IEventConsumer>();

                return new EventConsumerFactory(n => allEventConsumers.FirstOrDefault(x => x.Name == n));
            });

            services.AddSingletonAs<RuleService>();
            services.AddSingletonAs<EdmModelBuilder>();
        }
    }
}
