// ==========================================================================
//  CurrentStorageDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Statistics.Models
{
    public sealed class CurrentStorageDto
    {
        /// <summary>
        /// The size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// The maximum allowed asset size.
        /// </summary>
        public long MaxAllowed { get; set; }
    }
}
