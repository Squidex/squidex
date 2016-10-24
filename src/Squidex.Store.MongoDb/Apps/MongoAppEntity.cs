// ==========================================================================
//  MongoAppEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using PinkParrot.Read.Apps;
using PinkParrot.Store.MongoDb.Utils;

namespace PinkParrot.Store.MongoDb.Apps
{
    public sealed class MongoAppEntity : MongoEntity, IAppEntity
    {
        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }
    }
}
