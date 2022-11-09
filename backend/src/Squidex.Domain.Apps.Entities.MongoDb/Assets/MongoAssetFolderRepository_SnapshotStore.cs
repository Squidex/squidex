// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public sealed partial class MongoAssetFolderRepository : ISnapshotStore<AssetFolderDomainObject.State>, IDeleter
{
    Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
    }

    IAsyncEnumerable<SnapshotResult<AssetFolderDomainObject.State>> ISnapshotStore<AssetFolderDomainObject.State>.ReadAllAsync(
        CancellationToken ct)
    {
        return Collection.Find(FindAll, Batching.Options).ToAsyncEnumerable(ct)
            .Select(x => new SnapshotResult<AssetFolderDomainObject.State>(x.DocumentId, x.ToState(), x.Version, true));
    }

    async Task<SnapshotResult<AssetFolderDomainObject.State>> ISnapshotStore<AssetFolderDomainObject.State>.ReadAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/ReadAsync"))
        {
            var existing =
                await Collection.Find(x => x.DocumentId == key)
                    .FirstOrDefaultAsync(ct);

            if (existing != null)
            {
                return new SnapshotResult<AssetFolderDomainObject.State>(existing.DocumentId, existing.ToState(), existing.Version);
            }

            return new SnapshotResult<AssetFolderDomainObject.State>(default, null!, EtagVersion.Empty);
        }
    }

    async Task ISnapshotStore<AssetFolderDomainObject.State>.WriteAsync(SnapshotWriteJob<AssetFolderDomainObject.State> job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/WriteAsync"))
        {
            var entityJob = job.As(MongoAssetFolderEntity.Create(job));

            await Collection.UpsertVersionedAsync(entityJob, ct);
        }
    }

    async Task ISnapshotStore<AssetFolderDomainObject.State>.WriteManyAsync(IEnumerable<SnapshotWriteJob<AssetFolderDomainObject.State>> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/WriteManyAsync"))
        {
            var updates = jobs.Select(MongoAssetFolderEntity.Create).Select(x =>
                new ReplaceOneModel<MongoAssetFolderEntity>(
                    Filter.Eq(y => y.DocumentId, x.DocumentId),
                    x)
                {
                    IsUpsert = true
                }).ToList();

            if (updates.Count == 0)
            {
                return;
            }

            await Collection.BulkWriteAsync(updates, BulkUnordered, ct);
        }
    }

    async Task ISnapshotStore<AssetFolderDomainObject.State>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/RemoveAsync"))
        {
            await Collection.DeleteOneAsync(x => x.DocumentId == key, ct);
        }
    }
}
