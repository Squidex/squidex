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
using Squidex.Read.Schemas;

namespace Squidex.Read.MongoDb.Schemas
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
        public string SecurityToken { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid SchemaId { get; set; }
    }
}
