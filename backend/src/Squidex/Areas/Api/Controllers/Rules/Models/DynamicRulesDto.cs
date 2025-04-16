// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class DynamicRulesDto : Resource
{
    /// <summary>
    /// The rules.
    /// </summary>
    public DynamicRuleDto[] Items { get; set; }

    /// <summary>
    /// The ID of the rule that is currently rerunning.
    /// </summary>
    public DomainId? RunningRuleId { get; set; }
}
