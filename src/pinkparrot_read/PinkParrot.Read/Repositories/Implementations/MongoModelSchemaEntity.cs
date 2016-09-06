// ==========================================================================
//  MongoModelSchemaEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PinkParrot.Read.Repositories.Implementations
{
    public sealed class MongoModelSchemaEntity : IModelSchemaEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public DateTime Created { get; set; }

        [BsonRequired]
        [BsonElement]
        public DateTime LastModified { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid TenantId { get; set; }
        
        [BsonRequired]
        [BsonElement]
        public bool IsDeleted { get; set; }

        [BsonRequired]
        [BsonElement]
        public BsonDocument Schema { get; set; }
    }
}
