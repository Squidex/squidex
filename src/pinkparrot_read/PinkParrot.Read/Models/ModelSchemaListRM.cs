// ==========================================================================
//  ModelSchemaRM.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PinkParrot.Infrastructure;

namespace PinkParrot.Read.Models
{
    public sealed class ModelSchemaListRM : IEntity, ITenantEntity
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
    }
}
