// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed record SimulatedRuleEvent
{
    public Guid EventId { get; init; }

    public string EventName { get; init; }

    public object Event { get; init; }

    public object? EnrichedEvent { get; init; }

    public string? ActionName { get; init; }

    public string? ActionData { get; init; }

    public string? Error { get; init; }

    public SkipReason SkipReason { get; init; }
}
