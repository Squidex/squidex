// ==========================================================================
//  MongoSchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed class MongoSchemaEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Name { get; set; }

        [BsonElement]
        [BsonRequired]
        public int Version { get; set; }

        [BsonElement]
        [BsonRequired]
        public Guid AppId { get; set; }

        [BsonJson]
        [BsonElement]
        [BsonRequired]
        public SchemaState State { get; set; }
    }
}
