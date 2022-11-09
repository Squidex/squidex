// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions;

public sealed class EventMessageWrapper : IPayloadWrapper
{
    private readonly IEnumerable<ISubscriptionEventCreator> subscriptionEventCreators;

    public Envelope<AppEvent> Event { get; }

    object IPayloadWrapper.Message => Event.Payload;

    public EventMessageWrapper(Envelope<AppEvent> @event, IEnumerable<ISubscriptionEventCreator> subscriptionEventCreators)
    {
        Event = @event;

        this.subscriptionEventCreators = subscriptionEventCreators;
    }

    public async ValueTask<object> CreatePayloadAsync()
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
