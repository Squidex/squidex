// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows.Execution;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed record JobResult
{
    public Rule Rule { get; init; }

    public FlowExecutionState<RuleFlowContext>? State { get; init; }

    public EnrichedEvent? EnrichedEvent { get; init; }

    public Exception? EnrichmentError { get; init; }

    required public string EventName { get; init; }

    required public SkipReason SkipReason { get; init; }

    public static JobResult Failed(string eventName, SkipReason reason, Rule rule, EnrichedEvent? @event = null, Exception? exception = null)
    {
        return new JobResult
        {
            Rule = rule,
            EnrichedEvent = @event,
            EnrichmentError = exception,
            EventName = eventName,
            SkipReason = reason,
        };
    }
}
