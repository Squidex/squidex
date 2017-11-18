// ==========================================================================
//  StorageUsageDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Controllers.Api.Statistics.Models
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
    }
}
