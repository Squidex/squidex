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
    public DomainId IndexedAppId { get; set; }

    public static void RegisterClassMap()
    {
        AssetItemClassMap.Register();

        BsonClassMap.TryRegisterClassMap<MongoAssetEntity>(cm =>
        {
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
                .SetElementName("fn")
                .SetIsRequired(true);

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
                .SetIsRequired(true);

            cm.MapProperty(x => x.Type)
                .SetElementName("at")
                .SetIsRequired(true);
        });
    }

    public Asset ToState()
    {
        return this;
    }

    public static MongoAssetEntity Create(SnapshotWriteJob<Asset> job)
    {
        var entity = SimpleMapper.Map(job.Value, new MongoAssetEntity());

        entity.IndexedAppId = job.Value.AppId.Id;

        return entity;
    }
}
