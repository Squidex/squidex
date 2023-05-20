// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

[OpenApiRequest]
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
    [FromQuery(Name = "scheduleFrom")]
    public Instant? ScheduledFrom { get; set; }

    /// <summary>
    /// The end of the schedule.
    /// </summary>
    [FromQuery(Name = "scheduleTo")]
    public Instant? ScheduledTo { get; set; }

    /// <summary>
    /// The ID of the referencing content item.
    /// </summary>
    [FromQuery(Name = "referencing")]
    public DomainId? Referencing { get; set; }

    /// <summary>
    /// The ID of the reference content item.
    /// </summary>
    [FromQuery(Name = "references")]
    public DomainId? References { get; set; }

    /// <summary>
    /// The optional json query.
    /// </summary>
    [FromQuery(Name = "q")]
    public string? JsonQuery { get; set; }

    public Q ToQuery(HttpRequest request)
    {
        var result = Q.Empty;

        if (!string.IsNullOrWhiteSpace(Ids))
        {
            result = result.WithIds(Ids);
        }
        else if (ScheduledFrom != null && ScheduledTo != null)
        {
            result = result.WithSchedule(ScheduledFrom.Value, ScheduledTo.Value);
        }
        else if (Referencing != null)
        {
            result = result.WithReferencing(Referencing.Value);
        }
        else if (References != null)
        {
            result = result.WithReference(References.Value);
        }
        else
        {
            throw new ValidationException(T.Get("contents.invalidAllQuery"));
        }

        if (JsonQuery != null)
        {
            result = result.WithJsonQuery(JsonQuery);
        }

        return result.WithODataQuery(request.Query.ToString());
    }
}
