// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models.Triggers;

public sealed class CronJobRuleTriggerDto : RuleTriggerDto
{
    /// <summary>
    /// The cron expression that defines the interval.
    /// </summary>
    [Required]
    public string CronExpression { get; init; }

    /// <summary>
    /// The optional timezone.
    /// </summary>
    public string? CronTimezone { get; init; }

    /// <summary>
    /// The value sent to the flow.
    /// </summary>
    public JsonValue Value { get; init; }

    public static CronJobRuleTriggerDto FromDomain(CronJobTrigger trigger)
    {
        return SimpleMapper.Map(trigger, new CronJobRuleTriggerDto());
    }

    public override RuleTrigger ToTrigger()
    {
        return SimpleMapper.Map(this, new CronJobTrigger());
    }
}
