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

public sealed partial class MongoAssetRepository : ISnapshotStore<AssetDomainObject.State>, IDeleter
{
    Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
    }

    IAsyncEnumerable<SnapshotResult<AssetDomainObject.State>> ISnapshotStore<AssetDomainObject.State>.ReadAllAsync(
        CancellationToken ct)
    {
        return Collection.Find(FindAll, Batching.Options).ToAsyncEnumerable(ct)
            .Select(x => new SnapshotResult<AssetDomainObject.State>(x.DocumentId, x.ToState(), x.Version));
    }

    async Task<SnapshotResult<AssetDomainObject.State>> ISnapshotStore<AssetDomainObject.State>.ReadAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetRepository/ReadAsync"))
        {
            var existing =
                await Collection.Find(x => x.DocumentId == key)
                    .FirstOrDefaultAsync(ct);

            if (existing != null)
            {
                return new SnapshotResult<AssetDomainObject.State>(existing.DocumentId, existing.ToState(), existing.Version);
            }

            return new SnapshotResult<AssetDomainObject.State>(default, null!, EtagVersion.Empty);
        }
    }

    async Task ISnapshotStore<AssetDomainObject.State>.WriteAsync(SnapshotWriteJob<AssetDomainObject.State> job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetRepository/WriteAsync"))
        {
            var entityJob = job.As(MongoAssetEntity.Create(job));

            await Collection.UpsertVersionedAsync(entityJob, ct);
        }
    }

    async Task ISnapshotStore<AssetDomainObject.State>.WriteManyAsync(IEnumerable<SnapshotWriteJob<AssetDomainObject.State>> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetRepository/WriteManyAsync"))
        {
            var updates = jobs.Select(MongoAssetEntity.Create).Select(x =>
                new ReplaceOneModel<MongoAssetEntity>(
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

    async Task ISnapshotStore<AssetDomainObject.State>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoAssetRepository/RemoveAsync"))
        {
            await Collection.DeleteOneAsync(x => x.DocumentId == key, null, ct);
        }
    }
}
