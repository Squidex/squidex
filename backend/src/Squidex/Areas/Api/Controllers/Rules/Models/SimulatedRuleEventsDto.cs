// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class SimulatedRuleEventsDto : Resource
    {
        /// <summary>
        /// The simulated rule events.
        /// </summary>
        [LocalizedRequired]
        public SimulatedRuleEventDto[] Items { get; set; }

        /// <summary>
        /// The total number of simulated rule events.
        /// </summary>
        public long Total { get; set; }

        public static SimulatedRuleEventsDto FromSimulatedRuleEvents(IList<SimulatedRuleEvent> events)
        {
            var result = new SimulatedRuleEventsDto
            {
                Total = events.Count,
                Items = events.Select(SimulatedRuleEventDto.FromSimulatedRuleEvent).ToArray()
            };

            return result;
        }
    }
}
