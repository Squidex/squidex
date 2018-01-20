// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Statistics.Models
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
