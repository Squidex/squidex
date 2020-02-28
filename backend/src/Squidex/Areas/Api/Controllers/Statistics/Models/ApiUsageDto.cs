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
        public double AverageMs { get; set; }

        public static ApiUsageDto FromUsage((DateTime Date, ApiStats Stats) dateStatistics)
        {
            var (date, stats) = dateStatistics;

            return new ApiUsageDto
            {
                Date = date,
                TotalBytes = stats.TotalBytes,
                TotalCalls = stats.TotalCalls,
                AverageMs = stats.AverageElapsed,
            };
        }
    }
}
