// ==========================================================================
//  MongoAssetEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed class MongoAssetEntity : IVersionedEntity<Guid>
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement]
        [BsonRequired]
        public AssetState State { get; set; }

        [BsonElement]
        [BsonRequired]
        public long Version { get; set; }
    }
}
