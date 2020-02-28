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
    public sealed class ApiUsageDto
    {
        /// <summary>
        /// The date when the usage was tracked.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The total number of API calls.
        /// </summary>
        public long TotalCalls { get; set; }

        /// <summary>
        /// The total number of bytes transferred.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// The average duration in milliseconds.
        /// </summary>
        public double AverageElapsedMs { get; set; }

        public static ApiUsageDto FromStats(ApiStats stats)
        {
            return new ApiUsageDto
            {
                Date = stats.Date,
                TotalBytes = stats.TotalBytes,
                TotalCalls = stats.TotalCalls,
                AverageElapsedMs = stats.AverageElapsedMs,
            };
        }
    }
}
