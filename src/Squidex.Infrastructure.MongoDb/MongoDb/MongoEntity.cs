// ==========================================================================
//  MongoEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace Squidex.Infrastructure.MongoDb
{
    public abstract class MongoEntity : IMongoEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant Created { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant LastModified { get; set; }
    }
}
