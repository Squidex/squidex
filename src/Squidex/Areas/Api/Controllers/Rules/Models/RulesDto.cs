// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public static RulesDto FromRules(IEnumerable<IRuleEntity> items, ApiController controller, string app)
        {
            var result = new RulesDto
            {
                Items = items.Select(x => RuleDto.FromRule(x, controller, app)).ToArray()
            };

            return result.CreateLinks(controller, app);
        }

        private RulesDto CreateLinks(ApiController controller, string app)
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
            }

            return this;
        }
    }
}
