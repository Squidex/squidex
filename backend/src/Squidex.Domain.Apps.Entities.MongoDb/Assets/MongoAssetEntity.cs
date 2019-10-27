// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed class MongoAssetEntity :
        IAssetEntity,
        IUpdateableEntityWithVersion,
        IUpdateableEntityWithCreatedBy,
        IUpdateableEntityWithLastModifiedBy
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement("_ai")]
        [BsonRepresentation(BsonType.String)]
        public Guid IndexedAppId { get; set; }

        [BsonRequired]
        [BsonElement("ct")]
        public Instant Created { get; set; }

        [BsonRequired]
        [BsonElement("mt")]
        public Instant LastModified { get; set; }

        [BsonRequired]
        [BsonElement("ai")]
        public NamedId<Guid> AppId { get; set; }

        [BsonRequired]
        [BsonElement("mm")]
        public string MimeType { get; set; }

        [BsonRequired]
        [BsonElement("fn")]
        public string FileName { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("fh")]
        public string FileHash { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("sl")]
        public string Slug { get; set; }

        [BsonRequired]
        [BsonElement("fs")]
        public long FileSize { get; set; }

        [BsonRequired]
        [BsonElement("fv")]
        public long FileVersion { get; set; }

        [BsonRequired]
        [BsonElement("im")]
        public bool IsImage { get; set; }

        [BsonRequired]
        [BsonElement("vs")]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement("pw")]
        public int? PixelWidth { get; set; }

        [BsonRequired]
        [BsonElement("ph")]
        public int? PixelHeight { get; set; }

        [BsonRequired]
        [BsonElement("cb")]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement("mb")]
        public RefToken LastModifiedBy { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("td")]
        public HashSet<string> Tags { get; set; }

        [BsonRequired]
        [BsonElement("dl")]
        public bool IsDeleted { get; set; }

        Guid IAssetInfo.AssetId
        {
            get { return Id; }
        }
    }
}
