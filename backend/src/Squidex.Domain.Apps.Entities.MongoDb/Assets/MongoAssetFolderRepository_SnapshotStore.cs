// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets;

public sealed partial class MongoAssetFolderRepository : ISnapshotStore<AssetFolder>, IDeleter
{
    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
    }

    IAsyncEnumerable<SnapshotResult<AssetFolder>> ISnapshotStore<AssetFolder>.ReadAllAsync(
        CancellationToken ct)
    {
        var documents = Collection.Find(FindAll, Batching.Options).ToAsyncEnumerable(ct);

        return documents.Select(x => new SnapshotResult<AssetFolder>(x.DocumentId, x.ToState(), x.Version, true));
    }

    async Task<SnapshotResult<AssetFolder>> ISnapshotStore<AssetFolder>.ReadAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/ReadAsync"))
        {
            var existing =
                await Collection.Find(x => x.DocumentId == key)
                    .FirstOrDefaultAsync(ct);

            if (existing != null)
            {
                return new SnapshotResult<AssetFolder>(existing.DocumentId, existing.ToState(), existing.Version);
            }

            return new SnapshotResult<AssetFolder>(default, null!, EtagVersion.Empty);
        }
    }

    async Task ISnapshotStore<AssetFolder>.WriteAsync(SnapshotWriteJob<AssetFolder> job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/WriteAsync"))
        {
            var entityJob = job.As(MongoAssetFolderEntity.Create(job));

            await Collection.UpsertVersionedAsync(entityJob, Field.Of<AssetFolder>(x => nameof(x.Version)), ct);
        }
    }

    async Task ISnapshotStore<AssetFolder>.WriteManyAsync(IEnumerable<SnapshotWriteJob<AssetFolder>> jobs,
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

    async Task ISnapshotStore<AssetFolder>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/RemoveAsync"))
        {
            await Collection.DeleteOneAsync(x => x.DocumentId == key, ct);
        }
    }
}
