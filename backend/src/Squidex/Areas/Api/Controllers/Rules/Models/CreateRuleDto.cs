// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Flows.Internal;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

[OpenApiRequest]
public sealed class CreateRuleDto
{
    /// <summary>
    /// The trigger properties.
    /// </summary>
    [LocalizedRequired]
    public RuleTriggerDto Trigger { get; set; }

    /// <summary>
    /// The action properties.
    /// </summary>
    [Obsolete("Use Flow property")]
    public RuleAction? Action { get; set; }

    /// <summary>
    /// The flow to describe the sequence of actions to perform.
    /// </summary>
    [LocalizedRequired]
    public FlowDefinitionDto Flow { get; set; }

    public Rule ToRule()
    {
        return new Rule { Trigger = Trigger.ToTrigger(), Flow = GetFlow()! };
    }

    public CreateRule ToCommand()
    {
        return new CreateRule { Trigger = Trigger?.ToTrigger(), Flow = GetFlow()! };
    }

    private FlowDefinition? GetFlow()
    {
        var flow = Flow?.ToDefinition();
#pragma warning disable CS0618 // Type or member is obsolete
        if (flow == null && Action != null)
        {
            flow = new FlowDefinition
            {
                Steps = new Dictionary<Guid, FlowStepDefinition>
                {
                    [Guid.Empty] = new FlowStepDefinition { Step = Action.ToFlowStep() },
                },
            };
        }
#pragma warning restore CS0618 // Type or member is obsolete
        return flow;
    }
}
