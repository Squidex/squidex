// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions
{
    internal sealed class EventMessageWrapper : IPayloadWrapper<EnrichedEvent>
    {
        private readonly IEnumerable<ISubscriptionEventCreator> subscriptionEventCreators;

        public Envelope<AppEvent> Event { get; }

        object IPayloadWrapper<EnrichedEvent>.Payload => Event.Payload;

        public EventMessageWrapper(Envelope<AppEvent> @event, IEnumerable<ISubscriptionEventCreator> subscriptionEventCreators)
        {
            Event = @event;

            this.subscriptionEventCreators = subscriptionEventCreators;
        }

        public async ValueTask<EnrichedEvent> CreatePayloadAsync()
        {
            foreach (var creator in subscriptionEventCreators)
            {
                if (!creator.Handles(Event.Payload))
                {
                    continue;
                }

                var result = await creator.CreateEnrichedEventsAsync(Event, default);

                if (result != null)
                {
                    return result;
                }
            }

            return null!;
        }
    }
}
