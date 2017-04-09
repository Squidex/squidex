using System;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Assets;

namespace Squidex.Read.MongoDb.Assets
{
    public sealed class MongoAssetEntity : MongoEntity, IAssetEntity
    {
        [BsonRequired]
        [BsonElement]
        public string MimeType { get; set; }

        [BsonRequired]
        [BsonElement]
        public string FileName { get; set; }

        [BsonRequired]
        [BsonElement]
        public long FileSize { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsImage { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement]
        public int? PixelWidth { get; set; }

        [BsonRequired]
        [BsonElement]
        public int? PixelHeight { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken LastModifiedBy { get; set; }
    }
}
