// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleEventsDto : Resource
    {
        /// <summary>
        /// The rule events.
        /// </summary>
        [LocalizedRequired]
        public RuleEventDto[] Items { get; set; }

        /// <summary>
        /// The total number of rule events.
        /// </summary>
        public long Total { get; set; }

        public static RuleEventsDto FromRuleEvents(IResultList<IRuleEventEntity> ruleEvents, Resources resources)
        {
            var result = new RuleEventsDto
            {
                Total = ruleEvents.Total,
                Items = ruleEvents.Select(x => RuleEventDto.FromRuleEvent(x, resources)).ToArray()
            };

            return result.CreateLinks(resources);
        }

        private RuleEventsDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<RulesController>(x => nameof(x.GetEvents), values));

            return this;
        }
    }
}
