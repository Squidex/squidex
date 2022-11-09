// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers;

public sealed class UsageRuleTriggerDto : RuleTriggerDto
{
    /// <summary>
    /// The number of monthly api calls.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// The number of days to check or null for the current month.
    /// </summary>
    [LocalizedRange(1, 30)]
    public int? NumDays { get; set; }

    public override RuleTrigger ToTrigger()
    {
        return SimpleMapper.Map(this, new UsageTrigger());
    }
}
