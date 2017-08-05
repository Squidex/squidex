// ==========================================================================
//  Formatter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Text;
using EventStore.ClientAPI;
using EventStoreData = EventStore.ClientAPI.EventData;

namespace Squidex.Infrastructure.CQRS.Events
{
    public static class Formatter
    {
        public static StoredEvent Read(ResolvedEvent resolvedEvent)
        {
            var @event = resolvedEvent.Event;

            var body = Encoding.UTF8.GetString(@event.Data);
            var meta = Encoding.UTF8.GetString(@event.Metadata);

            var eventData = new EventData { Type = @event.EventType, EventId = @event.EventId, Payload = body, Metadata = meta };

            return new StoredEvent(
                resolvedEvent.OriginalEventNumber.ToString(),
                resolvedEvent.Event.EventNumber,
                eventData);
        }

        public static EventStoreData Write(EventData eventData)
        {
            var body = Encoding.UTF8.GetBytes(eventData.Payload);
            var meta = Encoding.UTF8.GetBytes(eventData.Metadata);

            return new EventStoreData(
                eventData.EventId,
                eventData.Type,
                true, body, meta);
        }
    }
}
