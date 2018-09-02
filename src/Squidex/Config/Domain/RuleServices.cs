// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Extensions.Actions;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Config.Domain
{
    public static class RuleServices
    {
        public static void AddMyRuleServices(this IServiceCollection services)
        {
            services.AddSingletonAs<EventEnricher>()
                .As<IEventEnricher>();

            services.AddSingletonAs<AssetChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<ContentChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<RuleEnqueuer>()
                .As<IEventConsumer>();

            services.AddSingletonAs<RuleEventFormatter>()
                .AsSelf();

            services.AddSingletonAs<RuleService>()
                .AsSelf();

            foreach (var actionHandler in RuleElementRegistry.ActionHandlers)
            {
                services.AddSingleton(typeof(IRuleActionHandler), actionHandler);
            }
        }
    }
}
