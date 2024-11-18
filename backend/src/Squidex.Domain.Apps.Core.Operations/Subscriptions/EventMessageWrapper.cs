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

public sealed class EventMessageWrapper(Envelope<AppEvent> @event, IEnumerable<ISubscriptionEventCreator> creators) : IPayloadWrapper
{
    public Envelope<AppEvent> Event { get; } = @event;

    object IPayloadWrapper.Message => Event.Payload;

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
