// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.Actions;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Apps.Services.Implementations;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.States;
using Squidex.Pipeline;

namespace Squidex.Config.Domain
{
    public static class ReadServices
    {
        public static void AddMyReadServices(this IServiceCollection services, IConfiguration config)
        {
            var consumeEvents = config.GetOptionalValue("eventStore:consume", false);

            if (consumeEvents)
            {
                services.AddTransient<EventConsumerGrain>();

                services.AddSingletonAs<EventConsumerGrainManager>()
                    .As<IRunnable>();
                services.AddSingletonAs<RuleDequeuer>()
                    .As<IRunnable>();
                services.AddSingletonAs<ContentScheduler>()
                    .As<IRunnable>();
            }

            var exposeSourceUrl = config.GetOptionalValue("assetStore:exposeSourceUrl", true);

            services.AddSingletonAs(c => new GraphQLUrlGenerator(
                    c.GetRequiredService<IOptions<MyUrlsOptions>>(),
                    c.GetRequiredService<IAssetStore>(),
                    exposeSourceUrl))
                .As<IGraphQLUrlGenerator>();

            services.AddSingletonAs<StateFactory>()
                .As<IInitializable>()
                .As<IStateFactory>();

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

            services.AddSingletonAs<AppProvider>()
                .As<IAppProvider>();

            services.AddSingletonAs<RuleEventFormatter>()
                .AsSelf();

            services.AddSingletonAs<AssetChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<ContentChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<AlgoliaActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingletonAs<AzureQueueActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingletonAs<ElasticSearchActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingletonAs<FastlyActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingletonAs<SlackActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingletonAs<WebhookActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingletonAs<DefaultEventNotifier>()
                .As<IEventNotifier>();

            services.AddSingletonAs<RuleEnqueuer>()
                .As<IEventConsumer>();

            services.AddSingletonAs(c =>
            {
                var allEventConsumers = c.GetServices<IEventConsumer>();

                return new EventConsumerFactory(n => allEventConsumers.FirstOrDefault(x => x.Name == n));
            });

            services.AddSingletonAs<EdmModelBuilder>()
                .AsSelf();

            services.AddSingletonAs<RuleService>()
                .AsSelf();
        }
    }
}
