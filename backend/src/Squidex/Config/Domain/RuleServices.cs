// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Queries;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Config.Domain
{
    public static class RuleServices
    {
        public static void AddSquidexRules(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<RuleOptions>(
                config.GetSection("rules"));

            services.AddTransientAs<RuleDomainObject>()
                .AsSelf();

            services.AddSingletonAs<EventEnricher>()
                .As<IEventEnricher>();

            services.AddSingletonAs<AssetChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<CommentTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<ContentChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<ManualTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<SchemaChangedTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<UsageTriggerHandler>()
                .As<IRuleTriggerHandler>();

            services.AddSingletonAs<RuleQueryService>()
                .As<IRuleQueryService>();

            services.AddSingletonAs<RuleEnricher>()
                .As<IRuleEnricher>();

            services.AddSingletonAs<RuleEnqueuer>()
                .As<IRuleEnqueuer>().As<IEventConsumer>();

            services.AddSingletonAs<RuleRegistry>()
                .As<ITypeProvider>().AsSelf();

            services.AddSingletonAs<RuleEventFormatter>()
                .AsSelf();

            services.AddSingletonAs<RuleService>()
                .AsSelf();

            services.AddSingletonAs<GrainBootstrap<IRuleDequeuerGrain>>()
                .AsSelf();
        }
    }
}
