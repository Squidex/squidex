// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.System;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Backup.Model;

public sealed class CompatibleStoredEvent
{
    [JsonPropertyName("n")]
    public NewEvent NewEvent { get; set; }

    [JsonPropertyName("streamName")]
    public string StreamName { get; set; }

    [JsonPropertyName("eventPosition")]
    public string EventPosition { get; set; }

    [JsonPropertyName("eventStreamNumber")]
    public long EventStreamNumber { get; set; }

    [JsonPropertyName("data")]
    public CompatibleEventData Data { get; set; }

    public static CompatibleStoredEvent V1(StoredEvent stored)
    {
        return new CompatibleStoredEvent
        {
            Data = CompatibleEventData.V1(stored.Data),
            EventPosition = stored.EventPosition,
            EventStreamNumber = stored.EventStreamNumber,
            StreamName = stored.StreamName
        };
    }

    public static CompatibleStoredEvent V2(StoredEvent stored)
    {
        return new CompatibleStoredEvent { NewEvent = NewEvent.V2(stored) };
    }

    public StoredEvent ToStoredEvent()
    {
        if (NewEvent != null)
        {
            return NewEvent.ToStoredEvent();
        }
        else
        {
            var data = Data.ToData();

            return new StoredEvent(StreamName, EventPosition, EventStreamNumber, data);
        }
    }
}

public sealed class CompatibleEventData
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("metadata")]
    public EnvelopeHeaders EventHeaders { get; set; }

    [JsonPropertyName("payload")]
    [JsonConverter(typeof(UnsafeRawJsonConverter))]
    public string EventPayload { get; set; }

    public static CompatibleEventData V1(EventData data)
    {
        return new CompatibleEventData
        {
            Type = data.Type,
            EventPayload = data.Payload,
            EventHeaders = data.Headers
        };
    }

    public EventData ToData()
    {
        return new EventData(Type, EventHeaders, EventPayload);
    }
}

public sealed class NewEvent
{
    [JsonPropertyName("t")]
    public string EventType { get; set; }

    [JsonPropertyName("s")]
    public string StreamName { get; set; }

    [JsonPropertyName("p")]
    public string EventPayload { get; set; }

    [JsonPropertyName("h")]
    public EnvelopeHeaders EventHeaders { get; set; }

    public static NewEvent V2(StoredEvent stored)
    {
        return new NewEvent
        {
            EventType = stored.Data.Type,
            EventHeaders = stored.Data.Headers,
            EventPayload = stored.Data.Payload,
            StreamName = stored.StreamName
        };
    }

    public StoredEvent ToStoredEvent()
    {
        var data = new EventData(EventType, EventHeaders, EventPayload);

        return new StoredEvent(StreamName, "0", -1, data);
    }
}
