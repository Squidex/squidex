// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class MongoUsage
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime Date { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Key { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string Category { get; set; }

        [BsonRequired]
        [BsonElement]
        public Counters Counters { get; set; } = new Counters();
    }
}
