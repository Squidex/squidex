// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleTypeProvider(IFlowStepRegistry flowStepRegistry) : ITypeProvider
{
    public void Map(TypeRegistry typeRegistry)
    {
        typeRegistry.Discriminator<DeprecatedRuleAction>("actionType");
        typeRegistry.Discriminator<FlowStep>("stepType");

        foreach (var (stepName, stepDefinition) in flowStepRegistry.Steps)
        {
            typeRegistry.Add<FlowStep>(stepDefinition.Type, stepName);
        }

        typeRegistry.Map(new AssemblyTypeProvider<EnrichedEvent>());
        typeRegistry.Map(new AssemblyTypeProvider<RuleTrigger>("triggerType"));
    }
}
