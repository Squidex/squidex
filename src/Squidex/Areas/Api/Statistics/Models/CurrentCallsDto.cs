// ==========================================================================
//  CurrentCallsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Statistics.Models
{
    public sealed class CurrentCallsDto
    {
        /// <summary>
        /// The number of calls.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The number of maximum allowed calls.
        /// </summary>
        public long MaxAllowed { get; set; }
    }
}
