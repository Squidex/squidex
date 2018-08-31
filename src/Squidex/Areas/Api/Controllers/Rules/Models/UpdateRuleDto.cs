// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class UpdateRuleDto
    {
        /// <summary>
        /// The trigger properties.
        /// </summary>
        public RuleTriggerDto Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        [JsonConverter(typeof(RuleActionSerializer))]
        public RuleAction Action { get; set; }

        public UpdateRule ToCommand(Guid id)
        {
            var command = new UpdateRule { RuleId = id, Action = Action };

            if (Trigger != null)
            {
                command.Trigger = Trigger.ToTrigger();
            }

            return command;
        }
    }
}
