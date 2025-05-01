// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class SimulatedRuleEventDto
{
    /// <summary>
    /// The unique event id.
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// The the unique id of the simulated event.
    /// </summary>
    public string UniqueId { get; set; }

    /// <summary>
    /// The name of the event.
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// The source event.
    /// </summary>
    public object Event { get; set; }

    // <summary>
    /// The enriched event.
    /// </summary>
    public object? EnrichedEvent { get; set; }

    /// <summary>
    /// The flow state.
    /// </summary>
    public FlowExecutionStateDto? FlowState { get; set; }

    /// <summary>
    /// The reason why the event has been skipped.
    /// </summary>
    public List<SkipReason> SkipReasons { get; set; } = [];

    public static SimulatedRuleEventDto FromDomain(SimulatedRuleEvent ruleEvent)
    {
        var result = SimpleMapper.Map(ruleEvent, new SimulatedRuleEventDto
        {
            FlowState = ruleEvent.State != null ? FlowExecutionStateDto.FromDomain(ruleEvent.State) : null,
        });

        foreach (var reason in Enum.GetValues<SkipReason>())
        {
            if (reason != SkipReason.None && ruleEvent.SkipReason.HasFlag(reason))
            {
                result.SkipReasons.Add(reason);
            }
        }

        return result;
    }
}
