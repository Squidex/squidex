// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
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
    [LocalizedRequired]
    public RuleAction Action { get; set; }

    public Rule ToRule()
    {
        return new Rule { Trigger = Trigger.ToTrigger(), Action = Action };
    }

    public CreateRule ToCommand()
    {
        return new CreateRule { Action = Action, Trigger = Trigger?.ToTrigger() };
    }
}
