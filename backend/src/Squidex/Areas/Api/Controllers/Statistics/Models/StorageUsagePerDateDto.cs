// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Areas.Api.Controllers.Statistics.Models
{
    public sealed class StorageUsagePerDateDto
    {
        /// <summary>
        /// The date when the usage was tracked.
        /// </summary>
        [JsonConverter(typeof(DateConverter))]
        public DateTime Date { get; set; }

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
                Date = stats.Date,
                TotalCount = stats.TotalCount,
                TotalSize = stats.TotalSize
            };

            return result;
        }
    }
}
