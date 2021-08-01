// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentsAllQueryDto
    {
        /// <summary>
        /// The list of ids to query.
        /// </summary>
        public List<DomainId>? Ids { get; set; }

        /// <summary>
        /// The list of ids to query.
        /// </summary>
        [FromQuery(Name = "ids")]
        public string? IdsString { get; set; }

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
            if (Ids != null)
            {
                return Q.Empty.WithIds(Ids);
            }
 
            if (!string.IsNullOrWhiteSpace(IdsString))
            {
                return Q.Empty.WithIds(IdsString);
            }

            if (ScheduledFrom != null && ScheduledTo != null)
            {
                return Q.Empty.WithSchedule(ScheduledFrom.Value, ScheduledTo.Value);
            }

            throw new ValidationException(T.Get("contents.invalidAllQuery"));
        }
    }
}
