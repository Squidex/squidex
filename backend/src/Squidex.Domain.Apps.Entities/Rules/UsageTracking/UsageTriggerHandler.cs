// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking;

public sealed class UsageTriggerHandler : IRuleTriggerHandler
{
    private const string EventName = "Usage exceeded";

    public Type TriggerType => typeof(UsageTrigger);

    public bool Handles(AppEvent appEvent)
    {
        return appEvent is AppUsageExceeded;
    }

    public async IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RuleContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var usageEvent = (AppUsageExceeded)@event.Payload;

        var result = new EnrichedUsageExceededEvent
        {
            CallsCurrent = usageEvent.CallsCurrent,
            CallsLimit = usageEvent.CallsLimit,
            Name = EventName
        };

        await Task.Yield();

        yield return result;
    }

    public bool Trigger(Envelope<AppEvent> @event, RuleContext context)
    {
        var trigger = (UsageTrigger)context.Rule.Trigger;

        var usageEvent = (AppUsageExceeded)@event.Payload;

        return usageEvent.CallsLimit >= trigger.Limit;
    }
}
