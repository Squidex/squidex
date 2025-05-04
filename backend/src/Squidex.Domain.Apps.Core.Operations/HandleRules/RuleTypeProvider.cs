// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleTypeProvider(IFlowStepRegistry flowStepRegistry, IOptions<RulesOptions> options) : ITypeProvider
{
    public void Map(TypeRegistry typeRegistry)
    {
        RegisterEvents(typeRegistry);
        RegisterSteps(typeRegistry);
        RegisterTriggers(typeRegistry);
#pragma warning disable CS0618 // Type or member is obsolete
        RegisterActions(typeRegistry);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private static void RegisterTriggers(TypeRegistry typeRegistry)
    {
        typeRegistry.Map(new AssemblyTypeProvider<RuleTrigger>("triggerType"));
    }

    private static void RegisterEvents(TypeRegistry typeRegistry)
    {
        typeRegistry.Map(new AssemblyTypeProvider<EnrichedEvent>());
    }

    private void RegisterSteps(TypeRegistry typeRegistry)
    {
        typeRegistry.Discriminator<FlowStep>("stepType");

        foreach (var (stepName, stepDefinition) in flowStepRegistry.Steps)
        {
            typeRegistry.Add<FlowStep>(stepDefinition.Type, stepName);
        }
    }

    [Obsolete("Has been replaced by flows.")]
    private void RegisterActions(TypeRegistry typeRegistry)
    {
        typeRegistry.Discriminator<RuleAction>("actionType");

        foreach (var actionType in options.Value.Actions)
        {
            var nameWithoutPRefix = actionType.TypeName(false, "Action", "ActionV2");

            typeRegistry.Add<RuleAction>(actionType, actionType.Name);
            typeRegistry.Add<RuleAction>(actionType, nameWithoutPRefix);
        }
    }
}
