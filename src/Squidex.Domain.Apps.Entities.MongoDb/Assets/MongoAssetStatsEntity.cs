// ==========================================================================
//  MongoAssetStatsEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Assets;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed class MongoAssetStatsEntity : IAssetStatsEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonDateTimeOptions(DateOnly = true)]
        public DateTime Date { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid AssetId { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalSize { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalCount { get; set; }
    }
}
