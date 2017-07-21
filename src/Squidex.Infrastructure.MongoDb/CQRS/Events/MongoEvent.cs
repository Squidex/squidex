// ==========================================================================
//  MongoEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class MongoEvent
    {
        [BsonElement]
        [BsonRequired]
        public Guid EventId { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Payload { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Metadata { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Type { get; set; }
    }
}