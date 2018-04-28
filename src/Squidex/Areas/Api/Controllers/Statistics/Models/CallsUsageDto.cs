// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class CallsUsageDto
    {
        /// <summary>
        /// The date when the usage was tracked.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The number of calls.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The average duration in milliseconds.
        /// </summary>
        public long AverageMs { get; set; }

        public static CallsUsageDto FromUsage(StoredUsage usage)
        {
            var averageMs = usage.TotalCount == 0 ? 0 : usage.TotalElapsedMs / usage.TotalCount;

            return new CallsUsageDto { Date = usage.Date, Count = usage.TotalCount, AverageMs = averageMs };
        }
    }
}
