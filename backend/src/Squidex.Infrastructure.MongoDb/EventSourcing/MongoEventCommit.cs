// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class MongoEventCommit
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public BsonTimestamp Timestamp { get; set; }

        [BsonElement]
        [BsonRequired]
        public MongoEvent[] Events { get; set; }

        [BsonElement]
        [BsonRequired]
        public long EventStreamOffset { get; set; }

        [BsonElement]
        [BsonRequired]
        public long EventsCount { get; set; }

        [BsonElement]
        [BsonRequired]
        public string EventStream { get; set; }
    }
}
