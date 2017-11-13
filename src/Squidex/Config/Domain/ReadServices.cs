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
using Squidex.Domain.Apps.Core.HandleRules.ActionHandlers;
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

            services.AddSingleton(c => new GraphQLUrlGenerator(
                    c.GetRequiredService<IOptions<MyUrlsOptions>>(),
                    c.GetRequiredService<IAssetStore>(),
                    exposeSourceUrl))
                .As<IGraphQLUrlGenerator>();

            services.AddSingleton(c => c.GetService<IOptions<MyUsageOptions>>()?.Value?.Plans.OrEmpty());

            services.AddSingleton<CachingGraphQLService>()
                .As<IGraphQLService>();

            services.AddSingleton<ContentQueryService>()
                .As<IContentQueryService>();

            services.AddSingleton<ConfigAppPlansProvider>()
                .As<IAppPlansProvider>();

            services.AddSingleton<AssetUserPictureStore>()
                .As<IUserPictureStore>();

            services.AddSingleton<AppHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingleton<ContentHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingleton<SchemaHistoryEventsCreator>()
                .As<IHistoryEventsCreator>();

            services.AddSingleton<NoopAppPlanBillingManager>()
                .As<IAppPlanBillingManager>();

            services.AddSingleton<OrleansEventNotifier>()
                .As<IEventNotifier>();

            services.AddSingleton<RuleDequeuer>()
                .As<IExternalSystem>();

            services.AddSingleton<OrleansAppProvider>()
                .As<IAppProvider>();

            services.AddSingleton<AppStateEventConsumer>()
                .As<IEventConsumer>();

            services.AddSingleton<RuleEnqueuer>()
                .As<IEventConsumer>();

            services.AddSingleton<ContentChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingleton<WebhookActionHandler>()
                .As<IRuleActionHandler>();

            services.AddSingleton<IEventConsumer>(c =>
                new CompoundEventConsumer(c.GetServices<IAssetEventConsumer>().ToArray()));

            services.AddSingleton(c =>
            {
                var allEventConsumers = c.GetServices<IEventConsumer>();

                return new EventConsumerFactory(n => allEventConsumers.FirstOrDefault(x => x.Name == n));
            });

            services.AddSingleton<RuleService>();
            services.AddSingleton<EdmModelBuilder>();
        }
    }
}
