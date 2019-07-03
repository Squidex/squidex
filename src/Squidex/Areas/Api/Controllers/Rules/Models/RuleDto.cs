// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Areas.Api.Controllers.Rules.Models.Converters;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleDto : Resource
    {
        /// <summary>
        /// The id of the rule.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user that has created the rule.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the rule.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The date and time when the rule has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The date and time when the rule has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The version of the rule.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Determines if the rule is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The trigger properties.
        /// </summary>
        [Required]
        public RuleTriggerDto Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        [Required]
        [JsonConverter(typeof(RuleActionConverter))]
        public RuleAction Action { get; set; }

        public static RuleDto FromRule(IRuleEntity rule, ApiController controller, string app)
        {
            var result = new RuleDto();

            SimpleMapper.Map(rule, result);
            SimpleMapper.Map(rule.RuleDef, result);

            if (rule.RuleDef.Trigger != null)
            {
                result.Trigger = RuleTriggerDtoFactory.Create(rule.RuleDef.Trigger);
            }

            return result.CreateLinks(controller, app);
        }

        private RuleDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app, id = Id };

            if (controller.HasPermission(Permissions.AppRulesDisable, app))
            {
                if (IsEnabled)
                {
                    AddPutLink("disable", controller.Url<RulesController>(x => nameof(x.DisableRule), values));
                }
                else
                {
                    AddPutLink("enable", controller.Url<RulesController>(x => nameof(x.EnableRule), values));
                }
            }

            if (controller.HasPermission(Permissions.AppRulesUpdate))
            {
                AddPutLink("update", controller.Url<RulesController>(x => nameof(x.PutRule), values));
            }

            if (controller.HasPermission(Permissions.AppRulesDelete))
            {
                AddDeleteLink("delete", controller.Url<RulesController>(x => nameof(x.DeleteRule), values));
            }

            return this;
        }
    }
}
