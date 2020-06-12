// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class CallsUsagePerDateDto
    {
        /// <summary>
        /// The date when the usage was tracked.
        /// </summary>
        [JsonConverter(typeof(DateConverter))]
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

        public static CallsUsagePerDateDto FromStats(ApiStats stats)
        {
            var result = new CallsUsagePerDateDto
            {
                Date = DateTime.SpecifyKind(stats.Date, DateTimeKind.Utc),
                TotalBytes = stats.TotalBytes,
                TotalCalls = stats.TotalCalls,
                AverageElapsedMs = stats.AverageElapsedMs
            };

            return result;
        }
    }
}
