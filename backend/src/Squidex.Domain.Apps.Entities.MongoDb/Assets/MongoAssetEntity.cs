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

public record MongoAssetEntity : Asset, IVersionedEntity<DomainId>
{
    public DomainId DocumentId { get; set; }

    public DomainId IndexedAppId { get; set; }

    public static void RegisterClassMap()
    {
        BsonClassMap.TryRegisterClassMap<MongoAssetEntity>(cm =>
        {
            cm.MapProperty(x => x.DocumentId)
                .SetElementName("_id")
                .SetIsRequired(true);

            cm.MapProperty(x => x.IndexedAppId)
                .SetElementName("_ai")
                .SetIsRequired(true);
        });

        BsonClassMap.TryRegisterClassMap<Asset>(cm =>
        {
            cm.MapProperty(x => x.FileName)
                .SetElementName("fn")
                .SetIsRequired(true);

            cm.MapProperty(x => x.FileHash)
                .SetElementName("fh")
                .SetIsRequired(false);

            cm.MapProperty(x => x.FileSize)
                .SetElementName("fs")
                .SetIsRequired(true);

            cm.MapProperty(x => x.FileVersion)
                .SetElementName("fv")
                .SetIsRequired(true);

            cm.MapProperty(x => x.IsProtected)
                .SetElementName("pt")
                .SetIgnoreIfDefault(true);

            cm.MapProperty(x => x.Metadata)
                .SetElementName("md")
                .SetIsRequired(true);

            cm.MapProperty(x => x.MimeType)
                .SetElementName("mm")
                .SetIsRequired(true);

            cm.MapProperty(x => x.Slug)
                .SetElementName("sl")
                .SetIsRequired(true);

            cm.MapProperty(x => x.Tags)
                .SetElementName("td")
                .SetIgnoreIfNull(true);

            cm.MapProperty(x => x.TotalSize)
                .SetElementName("ts")
                .SetIsRequired(false);

            cm.MapProperty(x => x.Type)
                .SetElementName("at")
                .SetIsRequired(true);
        });

        AssetItemClassMap.Register();
    }

    public Asset ToState()
    {
        return this;
    }

    public static MongoAssetEntity Create(SnapshotWriteJob<Asset> job)
    {
        var entity = new MongoAssetEntity
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
