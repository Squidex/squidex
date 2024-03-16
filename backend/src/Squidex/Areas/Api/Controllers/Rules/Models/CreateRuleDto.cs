// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class CreateRuleDto
    {
        /// <summary>
        /// The trigger properties.
        /// </summary>
        [LocalizedRequired]
        public RuleTriggerDto Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        [LocalizedRequired]
        [JsonConverter(typeof(RuleActionConverter))]
        public RuleAction Action { get; set; }

        public CreateRule ToCommand()
        {
            var command = new CreateRule { Action = Action };

            if (Trigger != null)
            {
                command.Trigger = Trigger.ToTrigger();
            }

            return command;
        }
    }
}
