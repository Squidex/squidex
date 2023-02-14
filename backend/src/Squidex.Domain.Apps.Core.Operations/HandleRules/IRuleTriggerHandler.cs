// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
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

    IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RulesContext context,
        CancellationToken ct);

    string? GetName(AppEvent @event)
    {
        return null;
    }

    bool Trigger(Envelope<AppEvent> @event, RuleTrigger trigger)
    {
        return true;
    }

    bool Trigger(EnrichedEvent @event, RuleTrigger trigger)
    {
        return true;
    }

    bool Handles(AppEvent @event);
}
