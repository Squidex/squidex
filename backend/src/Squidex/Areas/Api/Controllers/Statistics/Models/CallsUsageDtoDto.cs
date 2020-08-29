// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure.UsageTracking;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class CallsUsageDtoDto
    {
        /// <summary>
        /// The total number of API calls.
        /// </summary>
        public long TotalCalls { get; set; }

        /// <summary>
        /// The total number of bytes transferred.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// The amount of calls that will block the app.
        /// </summary>
        public long BlockingApiCalls { get; set; }

        /// <summary>
        /// The included API traffic.
        /// </summary>
        public long AllowedBytes { get; set; }

        /// <summary>
        /// The included API calls.
        /// </summary>
        public long AllowedCalls { get; set; }

        /// <summary>
        /// The average duration in milliseconds.
        /// </summary>
        public double AverageElapsedMs { get; set; }

        /// <summary>
        /// The statistics by date and group.
        /// </summary>
        [Required]
        public Dictionary<string, CallsUsagePerDateDto[]> Details { get; set; }

        public static CallsUsageDtoDto FromStats(IAppLimitsPlan plan, ApiStatsSummary summary, Dictionary<string, List<ApiStats>> details)
        {
            return new CallsUsageDtoDto
            {
                AverageElapsedMs = summary.AverageElapsedMs,
                BlockingApiCalls = plan.BlockingApiCalls,
                AllowedBytes = plan.MaxApiBytes,
                AllowedCalls = plan.MaxApiCalls,
                TotalBytes = summary.TotalBytes,
                TotalCalls = summary.TotalCalls,
                Details = details.ToDictionary(x => x.Key, x => x.Value.Select(CallsUsagePerDateDto.FromStats).ToArray())
            };
        }
    }
}
