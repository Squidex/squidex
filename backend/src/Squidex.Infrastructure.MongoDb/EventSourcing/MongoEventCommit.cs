// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.EventSourcing;

public sealed class MongoEventCommit
{
    [BsonId]
    [BsonElement]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Timestamp))]
    public BsonTimestamp Timestamp { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Events))]
    public MongoEvent[] Events { get; set; }

    [BsonRequired]
    [BsonElement(nameof(EventStreamOffset))]
    public long EventStreamOffset { get; set; }

    [BsonRequired]
    [BsonElement(nameof(EventsCount))]
    public long EventsCount { get; set; }

    [BsonRequired]
    [BsonElement(nameof(EventStream))]
    public string EventStream { get; set; }
}
