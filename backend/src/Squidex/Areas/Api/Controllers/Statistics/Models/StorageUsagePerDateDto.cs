// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;
using Squidex.Domain.Apps.Entities.Assets;

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class StorageUsagePerDateDto
    {
        /// <summary>
        /// The date when the usage was tracked.
        /// </summary>
        public LocalDate Date { get; set; }

        /// <summary>
        /// The number of assets.
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// The size in bytes.
        /// </summary>
        public long TotalSize { get; set; }

        public static StorageUsagePerDateDto FromStats(AssetStats stats)
        {
            var result = new StorageUsagePerDateDto
            {
                Date = LocalDate.FromDateTime(DateTime.SpecifyKind(stats.Date, DateTimeKind.Utc)),
                TotalCount = stats.TotalCount,
                TotalSize = stats.TotalSize
            };

            return result;
        }
    }
}
