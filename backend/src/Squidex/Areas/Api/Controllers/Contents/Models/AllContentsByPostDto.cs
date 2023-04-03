// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models;

[OpenApiRequest]
public sealed class AllContentsByPostDto
{
    /// <summary>
    /// The list of ids to query.
    /// </summary>
    public DomainId[]? Ids { get; set; }

    /// <summary>
    /// The start of the schedule.
    /// </summary>
    public Instant? ScheduledFrom { get; set; }

    /// <summary>
    /// The end of the schedule.
    /// </summary>
    public Instant? ScheduledTo { get; set; }

    /// <summary>
    /// The ID of the referencing content item.
    /// </summary>
    public DomainId? Referencing { get; set; }

    /// <summary>
    /// The ID of the reference content item.
    /// </summary>
    public DomainId? References { get; set; }

    /// <summary>
    /// The optional odata query.
    /// </summary>
    public string? OData { get; set; }

    /// <summary>
    /// The optional json query.
    /// </summary>
    [JsonPropertyName("q")]
    public JsonDocument? JsonQuery { get; set; }

    public Q ToQuery()
    {
        var result = Q.Empty;

        if (Ids?.Length > 0)
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
            result = result.WithJsonQuery(JsonQuery.RootElement.ToString());
        }

        return result.WithODataQuery(OData);
    }
}
