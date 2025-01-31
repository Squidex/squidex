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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed partial class EFAssetFolderRepository<TContext> : ISnapshotStore<AssetFolder>, IDeleter
{
    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFAssetFolderEntity>().Where(x => x.IndexedAppId == app.Id)
            .ExecuteDeleteAsync(ct);
    }

    async Task ISnapshotStore<AssetFolder>.ClearAsync(
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFAssetFolderEntity>()
            .ExecuteDeleteAsync(ct);
    }

    async Task ISnapshotStore<AssetFolder>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetFolderRepository/RemoveAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            await dbContext.Set<EFAssetFolderEntity>().Where(x => x.DocumentId == key)
                .ExecuteDeleteAsync(ct);
        }
    }

    async IAsyncEnumerable<SnapshotResult<AssetFolder>> ISnapshotStore<AssetFolder>.ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entities = dbContext.Set<EFAssetFolderEntity>().ToAsyncEnumerable();

        await foreach (var entity in entities.WithCancellation(ct))
        {
            yield return new SnapshotResult<AssetFolder>(entity.DocumentId, entity, entity.Version);
        }
    }

    async Task<SnapshotResult<AssetFolder>> ISnapshotStore<AssetFolder>.ReadAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetFolderRepository/ReadAsync"))
        {
            await using var dbContext = await CreateDbContextAsync(ct);

            var entity = await dbContext.Set<EFAssetFolderEntity>().Where(x => x.DocumentId == key).FirstOrDefaultAsync(ct);
            if (entity == null)
            {
                return new SnapshotResult<AssetFolder>(default, default!, EtagVersion.Empty);
            }

            return new SnapshotResult<AssetFolder>(entity.DocumentId, entity, entity.Version);
        }
    }

    async Task ISnapshotStore<AssetFolder>.WriteAsync(SnapshotWriteJob<AssetFolder> job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetFolderRepository/WriteAsync"))
        {
            var entity = EFAssetFolderEntity.Create(job);

            await using var dbContext = await CreateDbContextAsync(ct);
            await dbContext.UpsertAsync(entity, job.OldVersion, BuildUpdate, ct);
        }
    }

    async Task ISnapshotStore<AssetFolder>.WriteManyAsync(IEnumerable<SnapshotWriteJob<AssetFolder>> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("EFAssetFolderRepository/WriteManyAsync"))
        {
            var entities = jobs.Select(EFAssetFolderEntity.Create).ToList();
            if (entities.Count == 0)
            {
                return;
            }

            await using var dbContext = await CreateDbContextAsync(ct);
            await dbContext.BulkInsertAsync(entities, cancellationToken: ct);
        }
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }

    private static Expression<Func<SetPropertyCalls<EFAssetFolderEntity>, SetPropertyCalls<EFAssetFolderEntity>>> BuildUpdate(EFAssetFolderEntity entity)
    {
        return b => b
            .SetProperty(x => x.AppId, entity.AppId)
            .SetProperty(x => x.Created, entity.Created)
            .SetProperty(x => x.CreatedBy, entity.CreatedBy)
            .SetProperty(x => x.FolderName, entity.FolderName)
            .SetProperty(x => x.IsDeleted, entity.IsDeleted)
            .SetProperty(x => x.LastModified, entity.LastModified)
            .SetProperty(x => x.LastModifiedBy, entity.LastModifiedBy)
            .SetProperty(x => x.ParentId, entity.ParentId)
            .SetProperty(x => x.Version, entity.Version);
    }
}
