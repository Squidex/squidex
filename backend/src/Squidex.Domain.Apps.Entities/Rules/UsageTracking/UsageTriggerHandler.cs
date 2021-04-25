// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTriggerHandler : IRuleTriggerHandler
    {
        private const string EventName = "Usage exceeded";

        public Type TriggerType => typeof(UsageTrigger);

        public async IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RuleContext context,
            [EnumeratorCancellation] CancellationToken ct)
        {
            if (@event.Payload is not AppUsageExceeded usageEvent)
            {
                yield break;
            }

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
            if (context.Rule.Trigger is not UsageTrigger trigger)
            {
                return false;
            }

            if (@event.Payload is not AppUsageExceeded usageEvent)
            {
                return false;
            }

            return usageEvent.CallsLimit >= trigger.Limit;
        }

        public bool Trigger(EnrichedEvent @event, RuleContext context)
        {
            return @event is EnrichedUsageExceededEvent;
        }
    }
}
