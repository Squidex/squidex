// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows;
using Squidex.Flows.Internal;

namespace Squidex.Domain.Apps.Core.Rules.Deprecated;

[Obsolete("Has been replaced by flows.")]
public abstract record RuleAction
{
    public abstract FlowStep ToFlowStep();

    public FlowDefinition ToFlowDefinition()
    {
        var stepId = Guid.Parse("12345678-abcd-4cde-babe-987654321000");

        return new FlowDefinition
        {
            Steps = new Dictionary<Guid, FlowStepDefinition>
            {
                [stepId] = new FlowStepDefinition
                {
                    Step = ToFlowStep(),
                },
            },
            InitialStepId = stepId,
        };
    }
}
