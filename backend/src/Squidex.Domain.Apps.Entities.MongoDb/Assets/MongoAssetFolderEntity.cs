// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public record MongoAssetFolderEntity : AssetFolder, IVersionedEntity<DomainId>
{
    public DomainId DocumentId { get; set; }

    public DomainId IndexedAppId { get; set; }

    public static void RegisterClassMap()
    {
        BsonClassMap.TryRegisterClassMap<MongoAssetFolderEntity>(cm =>
        {
            cm.MapProperty(x => x.DocumentId)
                .SetElementName("_id")
                .SetIsRequired(true);

            cm.MapProperty(x => x.IndexedAppId)
                .SetElementName("_ai")
                .SetIsRequired(true);
        });

        BsonClassMap.TryRegisterClassMap<AssetFolder>(cm =>
        {
            cm.MapProperty(x => x.FolderName)
                .SetElementName("fn")
                .SetIsRequired(true);
        });

        AssetItemClassMap.Register();
    }

    public AssetFolder ToState()
    {
        return this;
    }

    public static MongoAssetFolderEntity Create(SnapshotWriteJob<AssetFolder> job)
    {
        var entity = new MongoAssetFolderEntity
        {
            DocumentId = job.Key,
            // Both version and ID cannot be changed by the mapper method anymore.
            Version = job.NewVersion,
            // Use an app ID without the name to reduce the memory usage of the index.
            IndexedAppId = job.Value.AppId.Id,
        };

        return SimpleMapper.Map(job.Value, entity);
    }
}
