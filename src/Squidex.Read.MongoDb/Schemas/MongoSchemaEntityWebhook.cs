// ==========================================================================
//  MongoSchemaEntityWebhook.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Read.Schemas;

namespace Squidex.Read.MongoDb.Schemas
{
    public sealed class MongoSchemaEntityWebhook : ISchemaWebhookEntity
    {
        [BsonRequired]
        [BsonElement]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public Uri Url { get; set; }

        [BsonRequired]
        [BsonElement]
        public string SecurityToken { get; set; }
    }
}
