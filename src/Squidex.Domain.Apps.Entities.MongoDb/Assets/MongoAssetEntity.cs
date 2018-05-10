// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed class MongoAssetEntity :
        MongoEntity,
        IAssetEntity,
        IUpdateableEntityWithVersion,
        IUpdateableEntityWithCreatedBy,
        IUpdateableEntityWithLastModifiedBy
    {
        [BsonRequired]
        [BsonElement("AppIdId")]
        public Guid IndexedAppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public NamedId<Guid> AppId { get; set; }

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
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement]
        public int? PixelWidth { get; set; }

        [BsonRequired]
        [BsonElement]
        public int? PixelHeight { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken LastModifiedBy { get; set; }

        [BsonElement]
        public bool IsDeleted { get; set; }

        Guid IAssetInfo.AssetId
        {
            get { return Id; }
        }
    }
}
