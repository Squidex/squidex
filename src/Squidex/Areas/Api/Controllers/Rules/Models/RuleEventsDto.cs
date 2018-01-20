// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Rules.Models
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
