// ==========================================================================
//  MongoEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PinkParrot.Read;

namespace PinkParrot.Store.MongoDb.Utils
{
    public abstract class MongoEntity : IEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public DateTime Created { get; set; }

        [BsonRequired]
        [BsonElement]
        public DateTime LastModified { get; set; }
    }
}
