// ==========================================================================
//  Formatter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Text;
using EventStore.ClientAPI;
using EventData = Squidex.Infrastructure.CQRS.Events.EventData;
using EventStoreData = EventStore.ClientAPI.EventData;

namespace Squidex.Infrastructure.GetEventStore
{
    public static class Formatter
    {
        public static EventData Read(RecordedEvent eventData)
        {
            var body = Encoding.UTF8.GetString(eventData.Data);
            var meta = Encoding.UTF8.GetString(eventData.Metadata);

            return new EventData { Type = eventData.EventType, EventId = eventData.EventId, Payload = body, Metadata = meta };
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
