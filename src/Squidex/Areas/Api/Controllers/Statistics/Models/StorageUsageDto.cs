// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Entities.Assets;

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class StorageUsageDto
    {
        /// <summary>
        /// The date when the usage was tracked.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The number of assets.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The size in bytes.
        /// </summary>
        public long Size { get; set; }

        public static StorageUsageDto FromStats(IAssetStatsEntity stats)
        {
            return new StorageUsageDto { Date = stats.Date, Count = stats.TotalCount, Size = stats.TotalSize };
        }
    }
}
