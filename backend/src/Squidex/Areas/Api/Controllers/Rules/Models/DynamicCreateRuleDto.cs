// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

[OpenApiRequest]
public sealed class DynamicCreateRuleDto
{
    /// <summary>
    /// The trigger properties.
    /// </summary>
    [LocalizedRequired]
    public RuleTriggerDto Trigger { get; set; }

    /// <summary>
    /// The action properties.
    /// </summary>
    [Obsolete("Use the new 'Flow' property to define actions")]
    public Dictionary<string, object>? Action { get; set; }

    /// <summary>
    /// The flow to describe the sequence of actions to perform.
    /// </summary>
    [LocalizedRequired]
    public DynamicFlowDefinitionDto Flow { get; set; }

    /// <summary>
    /// Enable or disable the rule.
    /// </summary>
    public bool? IsEnabled { get; set; }
}
