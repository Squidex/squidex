// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
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

        public Q ToQuery()
        {
            if (Ids?.Length > 0)
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
}
