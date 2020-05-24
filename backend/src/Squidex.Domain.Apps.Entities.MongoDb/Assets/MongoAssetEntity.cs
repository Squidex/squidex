// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed class MongoAssetEntity : IAssetEntity, IVersionedEntity<string>
    {
        [BsonId]
        [BsonElement("_id")]
        public string DocumentId { get; set; }

        [BsonRequired]
        [BsonElement("_ai")]
        public string IndexedAppId { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("ai")]
        public string AssetId { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("pi")]
        public string ParentId { get; set; }

        [BsonRequired]
        [BsonElement("ct")]
        public Instant Created { get; set; }

        [BsonRequired]
        [BsonElement("mt")]
        public Instant LastModified { get; set; }

        [BsonRequired]
        [BsonElement("an")]
        public string AppName { get; set; }

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
        [BsonElement("vs")]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement("at")]
        public AssetType Type { get; set; }

        [BsonRequired]
        [BsonElement("cb")]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement("mb")]
        public RefToken LastModifiedBy { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("td")]
        public HashSet<string> Tags { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("pt")]
        public bool IsProtected { get; set; }

        [BsonRequired]
        [BsonElement("dl")]
        public bool IsDeleted { get; set; }

        [BsonJson]
        [BsonRequired]
        [BsonElement("md")]
        public AssetMetadata Metadata { get; set; }

        DomainId IEntity.Id
        {
            get { return AssetId; }
        }

        DomainId IAssetInfo.AssetId
        {
            get { return ParentId; }
        }

        DomainId IAssetEntity.ParentId
        {
            get { return ParentId; }
        }

        NamedId<DomainId> IAssetEntity.AppId
        {
            get { return NamedId.Of(new DomainId(AssetId), ParentId);  }
        }
    }
}
