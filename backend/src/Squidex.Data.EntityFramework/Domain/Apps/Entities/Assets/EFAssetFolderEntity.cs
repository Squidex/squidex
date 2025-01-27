// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Assets;

[Table("AssetFolders")]
[Index(nameof(IndexedAppId), nameof(Id))]
public sealed record EFAssetFolderEntity : AssetFolder, IVersionedEntity<DomainId>
{
    [Key]
    public DomainId DocumentId { get; set; }

    public DomainId IndexedAppId { get; set; }

    public static EFAssetFolderEntity Create(SnapshotWriteJob<AssetFolder> job)
    {
        var entity = new EFAssetFolderEntity
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
