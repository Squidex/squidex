// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RulesDto : Resource
    {
        /// <summary>
        /// The rules.
        /// </summary>
        [LocalizedRequired]
        public RuleDto[] Items { get; set; }

        /// <summary>
        /// The id of the rule that is currently rerunning.
        /// </summary>
        public DomainId? RunningRuleId { get; set; }

        public static async Task<RulesDto> FromRulesAsync(IEnumerable<IEnrichedRuleEntity> items, IRuleRunnerService ruleRunnerService, Resources resources)
        {
            var runningRuleId = await ruleRunnerService.GetRunningRuleIdAsync(resources.Context.App.Id);

            var result = new RulesDto
            {
                Items = items.Select(x => RuleDto.FromRule(x, runningRuleId == null, ruleRunnerService, resources)).ToArray()
            };

            result.RunningRuleId = runningRuleId;

            return result.CreateLinks(resources, runningRuleId);
        }

        private RulesDto CreateLinks(Resources resources, DomainId? runningRuleId)
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
