// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using EventStore.ClientAPI;
using Squidex.Infrastructure.Json;
using EventStoreData = EventStore.ClientAPI.EventData;

namespace Squidex.Infrastructure.EventSourcing
{
    public static class Formatter
    {
        public static StoredEvent Read(ResolvedEvent resolvedEvent, IJsonSerializer serializer)
        {
            var @event = resolvedEvent.Event;

            var metadata = Encoding.UTF8.GetString(@event.Data);

            var headersJson = Encoding.UTF8.GetString(@event.Metadata);
            var headers = serializer.Deserialize<EnvelopeHeaders>(headersJson);

            var eventData = new EventData(@event.EventType, headers, metadata);

            return new StoredEvent(
                @event.EventStreamId,
                resolvedEvent.OriginalEventNumber.ToString(),
                resolvedEvent.Event.EventNumber,
                eventData);
        }

        public static EventStoreData Write(EventData eventData, IJsonSerializer serializer)
        {
            var payload = Encoding.UTF8.GetBytes(eventData.Payload);

            var headersJson = serializer.Serialize(eventData.Headers);
            var headersBytes = Encoding.UTF8.GetBytes(headersJson);

            return new EventStoreData(Guid.NewGuid(), eventData.Type, true, payload, headersBytes);
        }
    }
}
