// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Rules;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleEventsDto
    {
        /// <summary>
        /// The rule events.
        /// </summary>
        [Required]
        public RuleEventDto[] Items { get; set; }

        /// <summary>
        /// The total number of rule events.
        /// </summary>
        public long Total { get; set; }

        public static RuleEventsDto FromRuleEvents(IReadOnlyList<IRuleEventEntity> items, long total)
        {
            return new RuleEventsDto { Total = total, Items = items.Select(RuleEventDto.FromRuleEvent).ToArray() };
        }
    }
}
