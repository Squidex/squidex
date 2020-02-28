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
        public long TotalCount { get; set; }

        /// <summary>
        /// The size in bytes.
        /// </summary>
        public long TotalSize { get; set; }

        public static StorageUsageDto FromStats(AssetStats stats)
        {
            var result = new StorageUsageDto
            {
                Date = stats.Date,
                TotalCount = stats.TotalCount,
                TotalSize = stats.TotalSize
            };

            return result;
        }
    }
}
