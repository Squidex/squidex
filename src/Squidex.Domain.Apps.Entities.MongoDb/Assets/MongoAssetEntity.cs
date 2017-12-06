// ==========================================================================
//  MongoAssetEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Assets.State;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed class MongoAssetEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRequired]
        public AssetState State { get; set; }

        [BsonElement]
        [BsonRequired]
        public int Version { get; set; }
    }
}
