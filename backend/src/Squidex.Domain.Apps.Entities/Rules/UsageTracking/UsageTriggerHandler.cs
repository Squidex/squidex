// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTriggerHandler : RuleTriggerHandler<UsageTrigger, AppUsageExceeded, EnrichedUsageExceededEvent>
    {
        private const string EventName = "Usage exceeded";

        protected override Task<EnrichedUsageExceededEvent?> CreateEnrichedEventAsync(Envelope<AppUsageExceeded> @event)
        {
            var result = new EnrichedUsageExceededEvent
            {
                CallsCurrent = @event.Payload.CallsCurrent,
                CallsLimit = @event.Payload.CallsLimit,
                Name = EventName
            };

            return Task.FromResult<EnrichedUsageExceededEvent?>(result);
        }

        protected override bool Trigger(EnrichedUsageExceededEvent @event, UsageTrigger trigger)
        {
            return @event.CallsLimit == trigger.Limit;
        }
    }
}
