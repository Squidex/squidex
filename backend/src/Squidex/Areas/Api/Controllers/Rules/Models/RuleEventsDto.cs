// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleEventsDto : Resource
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

        public static RuleEventsDto FromRuleEvents(IResultList<IRuleEventEntity> ruleEvents, ApiController controller, string app)
        {
            var result = new RuleEventsDto
            {
                Total = ruleEvents.Total,
                Items = ruleEvents.Select(x => RuleEventDto.FromRuleEvent(x, controller, app)).ToArray()
            };

            return result.CreateLinks(controller, app);
        }

        private RuleEventsDto CreateLinks(ApiController controller, string app)
        {
            AddSelfLink(controller.Url<RulesController>(x => nameof(x.GetEvents), new { app }));

            return this;
        }
    }
}
