﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Migrations.OldTriggers;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.Extensions;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Queries;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Config.Domain;

public static class RuleServices
{
    public static void AddSquidexRules(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RulesOptions>(config,
            "rules");

        services.AddSingletonAs<EventEnricher>()
            .As<IEventEnricher>();

        services.AddSingletonAs<AssetChangedTriggerHandler>()
            .As<IRuleTriggerHandler>().As<ISubscriptionEventCreator>();

        services.AddSingletonAs<CommentTriggerHandler>()
            .As<IRuleTriggerHandler>();

        services.AddSingletonAs<ContentChangedTriggerHandler>()
            .As<IRuleTriggerHandler>().As<ISubscriptionEventCreator>();

        services.AddSingletonAs<AssetsFluidExtension>()
            .As<IFluidExtension>();

        services.AddSingletonAs<AssetsJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<ReferencesFluidExtension>()
            .As<IFluidExtension>();

        services.AddSingletonAs<ReferencesJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<ManualTriggerHandler>()
            .As<IRuleTriggerHandler>();

        services.AddSingletonAs<CronJobTriggerHandler>()
            .As<IRuleTriggerHandler>();

        services.AddSingletonAs<SchemaChangedTriggerHandler>()
            .As<IRuleTriggerHandler>();

        services.AddSingletonAs<UsageTriggerHandler>()
            .As<IRuleTriggerHandler>();

        services.AddSingletonAs<RuleQueryService>()
            .As<IRuleQueryService>();

        services.AddSingletonAs<DefaultRuleRunnerService>()
            .As<IRuleRunnerService>();

        services.AddSingletonAs<RuleEnricher>()
            .As<IRuleEnricher>();

        services.AddSingletonAs<RuleValidator>()
            .As<IRuleValidator>();

        services.AddSingletonAs<RuleEnqueuer>()
            .As<IRuleEnqueuer>().As<IEventConsumer>();

        services.AddSingletonAs<CronJobUpdater>()
            .As<IEventConsumer>();

        services.AddSingletonAs<EventJsonSchemaGenerator>()
            .AsSelf();

        services.AddSingletonAs(c => new AssemblyTypeProvider<RuleTrigger>(typeof(ContentChangedTrigger).Assembly, "triggerType"))
            .As<ITypeProvider>();

        services.AddSingletonAs<RuleTypeProvider>()
            .As<ITypeProvider>().AsSelf();

        services.AddSingletonAs<EventJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<EventFluidExtensions>()
            .As<IFluidExtension>();

        services.AddSingletonAs<PredefinedPatternsFormatter>()
            .As<IRuleEventFormatter>();

        services.AddSingletonAs<SimpleFormatter>()
            .AsSelf();

        services.AddSingletonAs<RuleService>()
            .As<IRuleService>();

        services.AddSingletonAs<RuleEventFormatter>()
            .As<IFlowExpressionEngine>();

        services.AddSingletonAs<RuleFlowTrackingCallback>()
            .As<IFlowExecutionCallback<FlowEventContext>>();

        services.AddFlows<FlowEventContext>(config);

        services.AddCronJobs<CronJobContext>(config, m =>
        {
            m.UpdateInterval = TimeSpan.FromSeconds(1);
        });
    }
}
