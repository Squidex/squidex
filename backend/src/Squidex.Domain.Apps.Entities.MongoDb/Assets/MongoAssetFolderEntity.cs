// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public sealed class MongoAssetFolderEntity : IAssetFolderEntity, IVersionedEntity<DomainId>
{
    [BsonId]
    [BsonElement("_id")]
    public DomainId DocumentId { get; set; }

    [BsonRequired]
    [BsonElement("_ai")]
    public DomainId IndexedAppId { get; set; }

    [BsonRequired]
    [BsonElement("id")]
    public DomainId Id { get; set; }

    [BsonRequired]
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
    [BsonElement("fn")]
    public string FolderName { get; set; }

    [BsonRequired]
    [BsonElement("vs")]
    public long Version { get; set; }

    [BsonRequired]
    [BsonElement("cb")]
    public RefToken CreatedBy { get; set; }

    [BsonRequired]
    [BsonElement("mb")]
    public RefToken LastModifiedBy { get; set; }

    [BsonRequired]
    [BsonElement("dl")]
    public bool IsDeleted { get; set; }

    public DomainId UniqueId
    {
        get => DocumentId;
    }

    public AssetFolderDomainObject.State ToState()
    {
        return SimpleMapper.Map(this, new AssetFolderDomainObject.State());
    }

    public static MongoAssetFolderEntity Create(SnapshotWriteJob<AssetFolderDomainObject.State> job)
    {
        var entity = SimpleMapper.Map(job.Value, new MongoAssetFolderEntity());

        entity.DocumentId = job.Key;
        entity.IndexedAppId = job.Value.AppId.Id;

        return entity;
    }
}
