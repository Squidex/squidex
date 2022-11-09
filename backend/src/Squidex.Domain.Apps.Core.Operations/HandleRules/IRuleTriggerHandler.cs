// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules;

public interface IRuleTriggerHandler
{
    Type TriggerType { get; }

    bool CanCreateSnapshotEvents
    {
        get => false;
    }

    IAsyncEnumerable<EnrichedEvent> CreateSnapshotEventsAsync(RuleContext context,
        CancellationToken ct)
    {
        return AsyncEnumerable.Empty<EnrichedEvent>();
    }

    IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RuleContext context,
        CancellationToken ct);

    string? GetName(AppEvent @event)
    {
        return null;
    }

    bool Trigger(Envelope<AppEvent> @event, RuleContext context)
    {
        return true;
    }

    bool Trigger(EnrichedEvent @event, RuleContext context)
    {
        return true;
    }

    bool Handles(AppEvent @event);
}
