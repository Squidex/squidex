// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class ApiUsagesDto
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
        /// The allowed API calls.
        /// </summary>
        public long AllowedCalls { get; set; }

        /// <summary>
        /// The average duration in milliseconds.
        /// </summary>
        public double AverageMs { get; set; }

        /// <summary>
        /// The statistics by date and group.
        /// </summary>
        [Required]
        public Dictionary<string, ApiUsageDto[]> Details { get; set; }

        public static ApiUsagesDto FromUsages(long allowedCalls, ApiStats summary, Dictionary<string, List<(DateTime Date, ApiStats Stats)>> details)
        {
            return new ApiUsagesDto
            {
                AllowedCalls = allowedCalls,
                AverageMs = summary.AverageElapsed,
                TotalBytes = summary.TotalBytes,
                TotalCalls = summary.TotalBytes,
                Details = details.ToDictionary(x => x.Key, x => x.Value.Select(ApiUsageDto.FromUsage).ToArray())
            };
        }
    }
}
