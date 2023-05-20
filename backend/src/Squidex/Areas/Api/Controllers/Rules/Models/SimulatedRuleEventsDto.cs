// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class SimulatedRuleEventsDto : Resource
{
    /// <summary>
    /// The total number of simulated rule events.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The simulated rule events.
    /// </summary>
    public SimulatedRuleEventDto[] Items { get; set; }

    public static SimulatedRuleEventsDto FromDomain(IList<SimulatedRuleEvent> events)
    {
        var result = new SimulatedRuleEventsDto
        {
            Total = events.Count,
            Items = events.Select(SimulatedRuleEventDto.FromDomain).ToArray()
        };

        return result;
    }
}
