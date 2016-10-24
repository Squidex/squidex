// ==========================================================================
//  MongoStreamPositionEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PinkParrot.Store.MongoDb.Infrastructure
{
    [DataContract]
    public class MongoStreamPositionEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement]
        public int? Position { get; set; }
    }
}