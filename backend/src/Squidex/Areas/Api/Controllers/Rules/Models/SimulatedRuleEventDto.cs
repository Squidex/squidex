// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed record SimulatedRuleEventDto
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        [Required]
        public string EventName { get; set; }

        /// <summary>
        /// The data for the action.
        /// </summary>
        public string? ActionName { get; set; }

        /// <summary>
        /// The name of the action.
        /// </summary>
        public string? ActionData { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// The reason why the event has been skipped.
        /// </summary>
        [Required]
        public SkipReason SkipReason { get; set; }

        public static SimulatedRuleEventDto FromSimulatedRuleEvent(SimulatedRuleEvent ruleEvent)
        {
            return SimpleMapper.Map(ruleEvent, new SimulatedRuleEventDto());
        }
    }
}
