// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RulesDto : Resource
    {
        /// <summary>
        /// The rules.
        /// </summary>
        [Required]
        public RuleDto[] Items { get; set; }

        /// <summary>
        /// The id of the rule that is currently rerunning.
        /// </summary>
        public Guid? RunningRuleId { get; set; }

        public static RulesDto FromRules(IEnumerable<IEnrichedRuleEntity> items, Guid? runningRuleId, Resources resources)
        {
            var result = new RulesDto
            {
                Items = items.Select(x => RuleDto.FromRule(x, runningRuleId, resources)).ToArray()
            };

            result.RunningRuleId = runningRuleId;

            return result.CreateLinks(resources, runningRuleId);
        }

        private RulesDto CreateLinks(Resources resources, Guid? runningRuleId)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<RulesController>(x => nameof(x.GetRules), values));

            if (resources.CanCreateRule)
            {
                AddPostLink("create", resources.Url<RulesController>(x => nameof(x.PostRule), values));
            }

            if (resources.CanReadRuleEvents)
            {
                AddGetLink("events", resources.Url<RulesController>(x => nameof(x.GetEvents), values));

                if (runningRuleId != null)
                {
                    AddDeleteLink("run/cancel", resources.Url<RulesController>(x => nameof(x.DeleteRuleRun), values));
                }
            }

            return this;
        }
    }
}
