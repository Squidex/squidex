// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Flows.Internal;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

[OpenApiRequest]
public sealed class UpdateRuleDto
{
    /// <summary>
    /// Optional rule name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The trigger properties.
    /// </summary>
    public RuleTriggerDto? Trigger { get; set; }

    /// <summary>
    /// The flow to describe the sequence of actions to perform.
    /// </summary>
    [Obsolete("Use Flow property")]
    public RuleAction? Action { get; set; }

    /// <summary>
    /// The flow.
    /// </summary>
    [LocalizedRequired]
    public FlowDefinitionDto Flow { get; set; }

    /// <summary>
    /// Enable or disable the rule.
    /// </summary>
    public bool? IsEnabled { get; set; }

    public UpdateRule ToCommand(DomainId id)
    {
        var command = SimpleMapper.Map(this, new UpdateRule { RuleId = id, Flow = GetFlow() });

        command.Trigger = Trigger?.ToTrigger();

        return command;
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
