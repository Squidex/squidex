// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
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
    [Obsolete("Use the new 'Flow' property to define actions")]
    public RuleAction? Action { get; set; }

    /// <summary>
    /// The flow.
    /// </summary>
    public FlowDefinitionDto? Flow { get; set; }

    /// <summary>
    /// Enable or disable the rule.
    /// </summary>
    public bool? IsEnabled { get; set; }

    public UpdateRule ToCommand(DomainId id)
    {
        return SimpleMapper.Map(this, new UpdateRule
        {
            RuleId = id,
#pragma warning disable CS0618 // Type or member is obsolete
            Flow = Flow?.ToDefinition() ?? Action?.ToFlowDefinition(),
#pragma warning restore CS0618 // Type or member is obsolete
            Trigger = Trigger?.ToTrigger(),
        });
    }
}
