// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventStore.ClientAPI;
using Squidex.Infrastructure.Json;
using EventStoreData = EventStore.ClientAPI.EventData;

namespace Squidex.Infrastructure.EventSourcing
{
    public static class Formatter
    {
        private static readonly HashSet<string> PrivateHeaders = new HashSet<string> { "$v", "$p", "$c", "$causedBy" };

        public static StoredEvent Read(ResolvedEvent resolvedEvent, string? prefix, IJsonSerializer serializer)
        {
            var @event = resolvedEvent.Event;

            var eventPayload = Encoding.UTF8.GetString(@event.Data);
            var eventHeaders = GetHeaders(serializer, @event);

            var eventData = new EventData(@event.EventType, eventHeaders, eventPayload);

            var streamName = GetStreamName(prefix, @event);

            return new StoredEvent(
                streamName,
                resolvedEvent.OriginalEventNumber.ToString(),
                resolvedEvent.Event.EventNumber,
                eventData);
        }

        private static string GetStreamName(string? prefix, RecordedEvent @event)
        {
            var streamName = @event.EventStreamId;

            if (prefix != null && streamName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                streamName = streamName[(prefix.Length + 1)..];
            }

            return streamName;
        }

        private static EnvelopeHeaders GetHeaders(IJsonSerializer serializer, RecordedEvent @event)
        {
            var headersJson = Encoding.UTF8.GetString(@event.Metadata);
            var headers = serializer.Deserialize<EnvelopeHeaders>(headersJson);

            foreach (var key in headers.Keys.ToList())
            {
                if (PrivateHeaders.Contains(key))
                {
                    headers.Remove(key);
                }
            }

            return headers;
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
