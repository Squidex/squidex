// ==========================================================================
//  MongoSchemaWebhookEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Read.Schemas;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Squidex.Domain.Apps.Read.MongoDb.Schemas
{
    public class MongoSchemaWebhookEntity : ISchemaWebhookEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public Uri Url { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid SchemaId { get; set; }

        [BsonRequired]
        [BsonElement]
        public string SharedSecret { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalSucceeded { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalFailed { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalTimedout { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalRequestTime { get; set; }
    }
}
