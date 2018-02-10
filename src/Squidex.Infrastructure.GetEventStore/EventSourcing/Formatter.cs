// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using EventStore.ClientAPI;
using EventStoreData = EventStore.ClientAPI.EventData;

namespace Squidex.Infrastructure.EventSourcing
{
    public static class Formatter
    {
        public static StoredEvent Read(ResolvedEvent resolvedEvent)
        {
            var @event = resolvedEvent.Event;

            var body = Encoding.UTF8.GetString(@event.Data);
            var meta = Encoding.UTF8.GetString(@event.Metadata);

            var eventData = new EventData { Type = @event.EventType, Payload = body, Metadata = meta };

            return new StoredEvent(
                resolvedEvent.OriginalEventNumber.ToString(),
                resolvedEvent.Event.EventNumber,
                eventData);
        }

        public static EventStoreData Write(EventData eventData)
        {
            var body = Encoding.UTF8.GetBytes(eventData.Payload.ToString());
            var meta = Encoding.UTF8.GetBytes(eventData.Metadata.ToString());

            return new EventStoreData(Guid.NewGuid(), eventData.Type, true, body, meta);
        }
    }
}
