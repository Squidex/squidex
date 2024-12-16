// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Flows.Internal;
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
    /// The flow that defines the actions.
    /// </summary>
    public FlowDefinition? Flow { get; set; }

    /// <summary>
    /// Enable or disable the rule.
    /// </summary>
    public bool? IsEnabled { get; set; }

    public UpdateRule ToCommand(DomainId id)
    {
        var command = SimpleMapper.Map(this, new UpdateRule { RuleId = id });

        command.Trigger = Trigger?.ToTrigger();

        return command;
    }
}
