// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public sealed class MongoAssetEntity : IAssetEntity, IVersionedEntity<DomainId>
{
    [BsonId]
    [BsonElement("_id")]
    public DomainId DocumentId { get; set; }

    [BsonRequired]
    [BsonElement("_ai")]
    public DomainId IndexedAppId { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("id")]
    public DomainId Id { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement("pi")]
    public DomainId ParentId { get; set; }

    [BsonRequired]
    [BsonElement("ai")]
    public NamedId<DomainId> AppId { get; set; }

    [BsonRequired]
    [BsonElement("ct")]
    public Instant Created { get; set; }

    [BsonRequired]
    [BsonElement("mt")]
    public Instant LastModified { get; set; }

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

    [BsonIgnoreIfDefault]
    [BsonElement("ts")]
    public long TotalSize { get; set; }

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

    [BsonRequired]
    [BsonElement("md")]
    public AssetMetadata Metadata { get; set; }

    public DomainId AssetId
    {
        get => Id;
    }

    public DomainId UniqueId
    {
        get => DocumentId;
    }

    public AssetDomainObject.State ToState()
    {
        return SimpleMapper.Map(this, new AssetDomainObject.State());
    }

    public static MongoAssetEntity Create(SnapshotWriteJob<AssetDomainObject.State> job)
    {
        var entity = SimpleMapper.Map(job.Value, new MongoAssetEntity());

        entity.DocumentId = job.Key;
        entity.IndexedAppId = job.Value.AppId.Id;

        return entity;
    }
}
