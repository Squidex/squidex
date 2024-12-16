// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RuleTypeProvider(IOptions<FlowOptions> options) : ITypeProvider
{
    public void Map(TypeRegistry typeRegistry)
    {
        typeRegistry.Discriminator<IFlowStep>("stepType");

        foreach (var type in options.Value.Steps)
        {
            var typeName = type.TypeName(false, "Step", "FlowStep");

            typeRegistry.Add<IFlowStep>(type, typeName);
        }

        typeRegistry.Map(new AssemblyTypeProvider<EnrichedEvent>());
        typeRegistry.Map(new AssemblyTypeProvider<RuleTrigger>("triggerType"));
    }
}
