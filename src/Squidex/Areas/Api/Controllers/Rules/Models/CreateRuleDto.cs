// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Rules.Commands;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class CreateRuleDto
    {
        /// <summary>
        /// The trigger properties.
        /// </summary>
        [Required]
        public RuleTriggerDto Trigger { get; set; }

        /// <summary>
        /// The action properties.
        /// </summary>
        [Required]
        public RuleActionDto Action { get; set; }

        public CreateRule ToCommand()
        {
            var command = new CreateRule();

            if (Action != null)
            {
                command.Action = Action.ToAction();
            }

            if (Trigger != null)
            {
                command.Trigger = Trigger.ToTrigger();
            }

            return command;
        }
    }
}
