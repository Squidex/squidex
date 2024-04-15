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
    private readonly IEnumerable<ISubscriptionEventCreator> creators;

    public Envelope<AppEvent> Event { get; }

    object IPayloadWrapper.Message => Event.Payload;

    public EventMessageWrapper(Envelope<AppEvent> @event, IEnumerable<ISubscriptionEventCreator> creators)
    {
        Event = @event;

        this.creators = creators;
    }

    public async ValueTask<object> CreatePayloadAsync()
    {
        foreach (var creator in creators)
        {
            if (!creator.Handles(Event.Payload))
            {
                continue;
            }

            if (await creator.CreateEnrichedEventsAsync(Event, default) is object result)
            {
                return result;
            }
        }

        return null!;
    }
}
