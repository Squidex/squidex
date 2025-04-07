// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows.Internal.Execution;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed record SimulatedRuleEvent
{
    public Guid EventId { get; init; }

    public string UniqueId { get; init; }

    public string EventName { get; init; }

    public object Event { get; init; }

    public string? Error { get; init; }

    public FlowExecutionState<FlowEventContext>? State { get; init; }

    public SkipReason SkipReason { get; init; }
}
