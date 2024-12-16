// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Old;

public abstract record RuleAction
{
    public abstract IFlowStep ToFlowStep();

    public FlowDefinition ToFlow()
    {
        var flowStep = ToFlowStep();
        var flowStepId = Guid.NewGuid();

        var flowDefinition = new FlowDefinition
        {
            InitialStep = flowStepId,
            Steps = new Dictionary<Guid, FlowStepDefinition>
            {
                [flowStepId] = new FlowStepDefinition { Step = flowStep }
            }
        };

        return flowDefinition;
    }
}

public abstract record RuleAction<T> : RuleAction where T : class, IFlowStep, new()
{
    public override IFlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new T());
    }
}
