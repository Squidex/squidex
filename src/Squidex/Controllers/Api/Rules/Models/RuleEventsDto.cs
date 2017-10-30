// ==========================================================================
//  RuleEventsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Rules.Models
{
    public sealed class RuleEventsDto
    {
        /// <summary>
        /// The total number of rule events.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The rule events.
        /// </summary>
        public RuleEventDto[] Items { get; set; }
    }
}
