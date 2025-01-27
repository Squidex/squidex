// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed partial class EFAssetRepository<TContext> : ISnapshotStore<Asset>, IDeleter
{
    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFAssetEntity>().Where(x => x.IndexedAppId == app.Id)
            .ExecuteDeleteAsync(ct);
    }

    async Task ISnapshotStore<Asset>.ClearAsync(
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFAssetEntity>()
            .ExecuteDeleteAsync(ct);
    }

    async Task ISnapshotStore<Asset>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/RemoveAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            await dbContext.Set<EFAssetEntity>().Where(x => x.DocumentId == key)
                .ExecuteDeleteAsync(ct);
        }
    }

    async IAsyncEnumerable<SnapshotResult<Asset>> ISnapshotStore<Asset>.ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entities = dbContext.Set<EFAssetEntity>().ToAsyncEnumerable();

        await foreach (var entity in entities.WithCancellation(ct))
        {
            yield return new SnapshotResult<Asset>(entity.DocumentId, entity, entity.Version);
        }
    }

    async Task<SnapshotResult<Asset>> ISnapshotStore<Asset>.ReadAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/ReadAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity = await dbContext.Set<EFAssetEntity>().Where(x => x.DocumentId == key).FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                return new SnapshotResult<Asset>(default, default!, EtagVersion.Empty);
            }

            return new SnapshotResult<Asset>(entity.DocumentId, entity, entity.Version);
        }
    }

    async Task ISnapshotStore<Asset>.WriteAsync(SnapshotWriteJob<Asset> job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/WriteAsync"))
        {
            var entity = EFAssetEntity.Create(job);

            await using var dbContext = await CreateDbContextAsync(ct);
            await dbContext.UpsertAsync(entity, job.OldVersion, BuildUpdate, ct);
        }
    }

    private static Expression<Func<SetPropertyCalls<EFAssetEntity>, SetPropertyCalls<EFAssetEntity>>> BuildUpdate(EFAssetEntity entity)
    {
        return b => b
            .SetProperty(x => x.AppId, entity.AppId)
            .SetProperty(x => x.Created, entity.Created)
            .SetProperty(x => x.CreatedBy, entity.CreatedBy)
            .SetProperty(x => x.FileHash, entity.FileHash)
            .SetProperty(x => x.FileName, entity.FileName)
            .SetProperty(x => x.FileSize, entity.FileSize)
            .SetProperty(x => x.FileVersion, entity.FileVersion)
            .SetProperty(x => x.IsDeleted, entity.IsDeleted)
            .SetProperty(x => x.IsProtected, entity.IsProtected)
            .SetProperty(x => x.LastModified, entity.LastModified)
            .SetProperty(x => x.LastModifiedBy, entity.LastModifiedBy)
            .SetProperty(x => x.Metadata, entity.Metadata)
            .SetProperty(x => x.MimeType, entity.MimeType)
            .SetProperty(x => x.ParentId, entity.ParentId)
            .SetProperty(x => x.Slug, entity.Slug)
            .SetProperty(x => x.Tags, entity.Tags)
            .SetProperty(x => x.TotalSize, entity.TotalSize)
            .SetProperty(x => x.Type, entity.Type)
            .SetProperty(x => x.Version, entity.Version);
    }

    async Task ISnapshotStore<Asset>.WriteManyAsync(IEnumerable<SnapshotWriteJob<Asset>> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetRepository/WriteManyAsync"))
        {
            var entities = jobs.Select(EFAssetEntity.Create).ToList();
            if (entities.Count == 0)
            {
                return;
            }

            await using var dbContext = await CreateDbContextAsync(ct);
            await dbContext.BulkInsertOrUpdateAsync(entities, cancellationToken: ct);
        }
    }
}
