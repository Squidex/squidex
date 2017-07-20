// ==========================================================================
//  MongoUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.MongoDb.UsageTracker
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

        [BsonRequired]
        [BsonElement]
        public double TotalCount { get; set; }

        [BsonRequired]
        [BsonElement]
        public double TotalElapsedMs { get; set; }
    }
}
