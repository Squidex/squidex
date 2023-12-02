// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public record MongoAssetFolderEntity : AssetFolder, IVersionedEntity<DomainId>
{
    [BsonRequired]
    [BsonElement("_ai")]
    public DomainId IndexedAppId { get; set; }

    public static void RegisterClassMap()
    {
        AssetItemClassMap.Register();

        BsonClassMap.TryRegisterClassMap<AssetFolder>(cm =>
        {
            cm.MapProperty(x => x.FolderName)
                .SetElementName("fn")
                .SetIsRequired(true);
        });
    }

    public AssetFolder ToState()
    {
        return this;
    }

    public static MongoAssetFolderEntity Create(SnapshotWriteJob<AssetFolder> job)
    {
        var entity = SimpleMapper.Map(job.Value, new MongoAssetFolderEntity());

        entity.IndexedAppId = job.Value.AppId.Id;

        return entity;
    }
}
