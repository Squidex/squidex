// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

public sealed class AllContentsByGetDto
{
    /// <summary>
    /// The list of ids to query.
    /// </summary>
    [FromQuery(Name = "ids")]
    public string? Ids { get; set; }

    /// <summary>
    /// The start of the schedule.
    /// </summary>
    [FromQuery]
    public Instant? ScheduledFrom { get; set; }

    /// <summary>
    /// The end of the schedule.
    /// </summary>
    [FromQuery]
    public Instant? ScheduledTo { get; set; }

    public Q ToQuery()
    {
        if (!string.IsNullOrWhiteSpace(Ids))
        {
            return Q.Empty.WithIds(Ids);
        }

        if (ScheduledFrom != null && ScheduledTo != null)
        {
            return Q.Empty.WithSchedule(ScheduledFrom.Value, ScheduledTo.Value);
        }

        throw new ValidationException(T.Get("contents.invalidAllQuery"));
    }
}
