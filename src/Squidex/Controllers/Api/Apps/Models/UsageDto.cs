// ==========================================================================
//  UsageDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Controllers.Api.Apps.Models
{
    public class UsageDto
    {
        /// <summary>
        /// The date when the usage was tracked.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The number of calls.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The average duration in milliseconds.
        /// </summary>
        public long AverageMs { get; set; }
    }
}
