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
    [LocalizedRequired]
    public Dictionary<string, object> Action { get; set; }
}
