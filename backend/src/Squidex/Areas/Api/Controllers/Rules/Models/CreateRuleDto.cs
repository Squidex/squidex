// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
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
    /// The flow that defines the actions.
    /// </summary>
    [LocalizedRequired]
    public FlowDefinition Flow { get; set; }

    public Rule ToRule()
    {
        return new Rule { Trigger = Trigger.ToTrigger(), Flow = Flow };
    }

    public CreateRule ToCommand()
    {
        return new CreateRule { Trigger = Trigger?.ToTrigger(), Flow = Flow };
    }
}
