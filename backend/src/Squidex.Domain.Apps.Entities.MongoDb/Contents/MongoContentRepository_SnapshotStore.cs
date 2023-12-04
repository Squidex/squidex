// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public partial class MongoContentRepository : ISnapshotStore<WriteContent>, IDeleter
{
    IAsyncEnumerable<SnapshotResult<WriteContent>> ISnapshotStore<WriteContent>.ReadAllAsync(
        CancellationToken ct)
    {
        return collectionComplete.StreamAll(ct)
            .Select(x => new SnapshotResult<WriteContent>(x.DocumentId, x.ToState(), x.Version, true));
    }

    async Task<SnapshotResult<WriteContent>> ISnapshotStore<WriteContent>.ReadAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentRepository/ReadAsync"))
        {
            var existing =
                await collectionComplete.FindAsync(key, ct);

            // Support for all versions, where we do not have full snapshots in the collection.
            if (existing?.IsSnapshot == true)
            {
                return new SnapshotResult<WriteContent>(existing.DocumentId, existing.ToState(), existing.Version);
            }

            return new SnapshotResult<WriteContent>(default, null!, EtagVersion.Empty);
        }
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentRepository/DeleteAppAsync"))
        {
            await collectionComplete.DeleteAppAsync(app.Id, ct);
            await collectionPublished.DeleteAppAsync(app.Id, ct);
        }
    }

    async Task ISnapshotStore<WriteContent>.ClearAsync(
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentRepository/ClearAsync"))
        {
            await collectionComplete.ClearAsync(ct);
            await collectionPublished.ClearAsync(ct);
        }
    }

    async Task ISnapshotStore<WriteContent>.RemoveAsync(DomainId key,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentRepository/RemoveAsync"))
        {
            // Some data is corrupt and might throw an exception if we do not ignore it.
            if (key == DomainId.Empty)
            {
                return;
            }

            await Task.WhenAll(
                collectionComplete.RemoveAsync(key, ct),
                collectionPublished.RemoveAsync(key, ct));
        }
    }

    async Task ISnapshotStore<WriteContent>.WriteAsync(SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentRepository/WriteAsync"))
        {
            // Some data is corrupt and might throw an exception if we do not ignore it.
            if (!IsValid(job.Value))
            {
                return;
            }

            if (!CanUseTransactions)
            {
                // If transactions are not supported we update the documents without version checks,
                // otherwise we would not be able to recover from inconsistencies.
                await Task.WhenAll(
                    UpsertCompleteAsync(job, default),
                    UpsertPublishedAsync(job, default));
                return;
            }

            using (var session = await database.Client.StartSessionAsync(cancellationToken: ct))
            {
                // Make an update with full transaction support to be more consistent.
                await session.WithTransactionAsync(async (session, ct) =>
                {
                    await UpsertVersionedCompleteAsync(session, job, ct);
                    await UpsertVersionedPublishedAsync(session, job, ct);
                    return true;
                }, null, ct);
            }
        }
    }

    async Task ISnapshotStore<WriteContent>.WriteManyAsync(IEnumerable<SnapshotWriteJob<WriteContent>> jobs,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentRepository/WriteManyAsync"))
        {
            var collectionUpdates = new Dictionary<IMongoCollection<MongoContentEntity>, List<MongoContentEntity>>();

            var add = new Action<IMongoCollection<MongoContentEntity>, MongoContentEntity>((collection, entity) =>
            {
                collectionUpdates.GetOrAddNew(collection).Add(entity);
            });

            foreach (var job in jobs)
            {
                var isValid = IsValid(job.Value);

                if (isValid && ShouldWritePublished(job.Value))
                {
                    await collectionPublished.AddCollectionsAsync(
                        await MongoContentEntity.CreatePublishedAsync(job, appProvider, ct), add, ct);
                }

                if (isValid)
                {
                    await collectionComplete.AddCollectionsAsync(
                        await MongoContentEntity.CreateCompleteAsync(job, appProvider, ct), add, ct);
                }
            }

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = ct,
                // This is just an estimate, but we do not want ot have unlimited parallelism.
                MaxDegreeOfParallelism = 8
            };

            // Make one update per collection.
            await Parallel.ForEachAsync(collectionUpdates, parallelOptions, (update, ct) =>
            {
                return new ValueTask(update.Key.InsertManyAsync(update.Value, InsertUnordered, ct));
            });
        }
    }

    private async Task UpsertPublishedAsync(SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        if (ShouldWritePublished(job.Value))
        {
            var entityJob = job.As(await MongoContentEntity.CreatePublishedAsync(job, appProvider, ct));

            await collectionPublished.UpsertAsync(entityJob, ct);
        }
        else
        {
            await collectionPublished.RemoveAsync(job.Key, ct);
        }
    }

    private async Task UpsertVersionedPublishedAsync(IClientSessionHandle session, SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        if (ShouldWritePublished(job.Value))
        {
            var entityJob = job.As(await MongoContentEntity.CreatePublishedAsync(job, appProvider, ct));

            await collectionPublished.UpsertVersionedAsync(session, entityJob, ct);
        }
        else
        {
            await collectionPublished.RemoveAsync(session, job.Key, ct);
        }
    }

    private async Task UpsertCompleteAsync(SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        var entityJob = job.As(await MongoContentEntity.CreateCompleteAsync(job, appProvider, ct));

        await collectionComplete.UpsertAsync(entityJob, ct);
    }

    private async Task UpsertVersionedCompleteAsync(IClientSessionHandle session, SnapshotWriteJob<WriteContent> job,
        CancellationToken ct)
    {
        var entityJob = job.As(await MongoContentEntity.CreateCompleteAsync(job, appProvider, ct));

        await collectionComplete.UpsertVersionedAsync(session, entityJob, ct);
    }

    private static bool ShouldWritePublished(WriteContent value)
    {
        // Only published content is written to the published collection.
        return value.CurrentVersion.Status == Status.Published && !value.IsDeleted;
    }

    private static bool IsValid(WriteContent state)
    {
        // Some data is corrupt and might throw an exception during migration if we do not skip them.
        return
            state.AppId != null &&
            state.AppId.Id != DomainId.Empty &&
            state.CurrentVersion != null &&
            state.SchemaId != null &&
            state.SchemaId.Id != DomainId.Empty;
    }
}
