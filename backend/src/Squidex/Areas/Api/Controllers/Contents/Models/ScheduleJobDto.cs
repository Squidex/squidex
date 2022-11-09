// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public sealed class ScheduleJobDto
{
    /// <summary>
    /// The ID of the schedule job.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The new status.
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// The target date and time when the content should be scheduled.
    /// </summary>
    public Instant DueTime { get; set; }

    /// <summary>
    /// The color of the scheduled status.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// The user who schedule the content.
    /// </summary>
    [LocalizedRequired]
    public RefToken ScheduledBy { get; set; }
}
