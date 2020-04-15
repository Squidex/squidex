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
using Squidex.Shared;
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

        public static RulesDto FromRules(IEnumerable<IEnrichedRuleEntity> items, Guid? runningRuleId, ApiController controller, string app)
        {
            var result = new RulesDto
            {
                Items = items.Select(x => RuleDto.FromRule(x, runningRuleId, controller, app)).ToArray()
            };

            result.RunningRuleId = runningRuleId;

            return result.CreateLinks(controller, runningRuleId, app);
        }

        private RulesDto CreateLinks(ApiController controller, Guid? runningRuleId, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<RulesController>(x => nameof(x.GetRules), values));

            if (controller.HasPermission(Permissions.AppRulesCreate, app))
            {
                AddPostLink("create", controller.Url<RulesController>(x => nameof(x.PostRule), values));
            }

            if (controller.HasPermission(Permissions.AppRulesEvents, app))
            {
                AddGetLink("events", controller.Url<RulesController>(x => nameof(x.GetEvents), values));

                if (runningRuleId != null)
                {
                    AddDeleteLink("run/cancel", controller.Url<RulesController>(x => nameof(x.DeleteRuleRun), values));
                }
            }

            return this;
        }
    }
}
