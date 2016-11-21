// ==========================================================================
//  MongoAppClientKeyEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Read.Apps;

namespace Squidex.Store.MongoDb.Apps
{
    public class MongoAppClientKeyEntity : IAppClientKeyEntity
    {
        [BsonRequired]
        [BsonElement]
        public string ClientKey { get; set; }

        [BsonRequired]
        [BsonElement]
        public DateTime ExpiresUtc { get; set; }
    }
}
