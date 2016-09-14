// ==========================================================================
//  MongoPosition.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PinkParrot.Read.Repositories.Implementations.Mongo
{
    [DataContract]
    public class MongoPosition
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement]
        public int? Position { get; set; }
    }
}