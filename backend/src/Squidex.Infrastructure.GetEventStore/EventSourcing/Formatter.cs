// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using EventStore.Client;
using Squidex.Infrastructure.Json;
using EventStoreData = EventStore.Client.EventData;

namespace Squidex.Infrastructure.EventSourcing;

public static class Formatter
{
    private static readonly HashSet<string> PrivateHeaders = new HashSet<string> { "$v", "$p", "$c", "$causedBy" };

    public static StoredEvent Read(ResolvedEvent resolvedEvent, string? prefix, IJsonSerializer serializer)
    {
        var @event = resolvedEvent.Event;

        var eventPayload = Encoding.UTF8.GetString(@event.Data.Span);
        var eventHeaders = GetHeaders(serializer, @event);

        var eventData = new EventData(@event.EventType, eventHeaders, eventPayload);

        var streamName = GetStreamName(prefix, @event);

        return new StoredEvent(
            streamName,
            resolvedEvent.OriginalEventNumber.ToInt64().ToString(CultureInfo.InvariantCulture),
            resolvedEvent.Event.EventNumber.ToInt64(),
            eventData);
    }

    private static string GetStreamName(string? prefix, EventRecord @event)
    {
        var streamName = @event.EventStreamId;

        if (prefix != null && streamName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            streamName = streamName[(prefix.Length + 1)..];
        }

        return streamName;
    }

    private static EnvelopeHeaders GetHeaders(IJsonSerializer serializer, EventRecord @event)
    {
        var headers = Deserialize<EnvelopeHeaders>(serializer, @event.Metadata);

        foreach (var key in headers.Keys.ToList())
        {
            if (PrivateHeaders.Contains(key))
            {
                headers.Remove(key);
            }
        }

        return headers;
    }

    private static T Deserialize<T>(IJsonSerializer serializer, ReadOnlyMemory<byte> source)
    {
        var json = Encoding.UTF8.GetString(source.Span);

        return serializer.Deserialize<T>(json);
    }

    public static EventStoreData Write(EventData eventData, IJsonSerializer serializer)
    {
        var payload = Encoding.UTF8.GetBytes(eventData.Payload);

        var headersJson = serializer.Serialize(eventData.Headers);
        var headersBytes = Encoding.UTF8.GetBytes(headersJson);

        return new EventStoreData(Uuid.FromGuid(Guid.NewGuid()), eventData.Type, payload, headersBytes);
    }
}
