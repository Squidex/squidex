using System;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Domain.Apps.Read.Assets;

namespace Squidex.Domain.Apps.Read.MongoDb.Assets
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
        public long FileVersion { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsImage { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsDeleted { get; set; }

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
