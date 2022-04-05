// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class UpdateRuleDto
    {
        /// <summary>
        /// Optional rule name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The trigger properties.
        /// </summary>
        public RuleTriggerDto? Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        [JsonConverter(typeof(RuleActionConverter))]
        public RuleAction? Action { get; set; }

        /// <summary>
        /// Enable or disable the rule.
        /// </summary>
        public bool? IsEnabled { get; set; }

        public UpdateRule ToCommand(DomainId id)
        {
            var command = SimpleMapper.Map(this, new UpdateRule { RuleId = id });

            if (Trigger != null)
            {
                command.Trigger = Trigger.ToTrigger();
            }

            return command;
        }
    }
}
