// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.EventSourcing;

public sealed class MongoEvent
{
    [BsonElement]
    [BsonRequired]
    public string Type { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Payload))]
    public string Payload { get; set; }

    [BsonRequired]
    [BsonElement("Metadata")]
    public EnvelopeHeaders Headers { get; set; }

    public static MongoEvent FromEventData(EventData data)
    {
        return new MongoEvent { Type = data.Type, Headers = data.Headers, Payload = data.Payload };
    }

    public EventData ToEventData()
    {
        return new EventData(Type, Headers, Payload);
    }
}
