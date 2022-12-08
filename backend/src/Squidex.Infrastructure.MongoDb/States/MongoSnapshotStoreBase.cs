// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States;

public abstract class MongoSnapshotStoreBase<T, TState> : MongoRepositoryBase<TState>, ISnapshotStore<T> where TState : MongoState<T>, new()
{
    protected MongoSnapshotStoreBase(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        var attribute = typeof(T).GetCustomAttributes(true).OfType<CollectionNameAttribute>().FirstOrDefault();

        var name = attribute?.Name ?? typeof(T).Name;

        return $"States_{name}";
    }

    public async Task<SnapshotResult<T>> ReadAsync(DomainId key,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSnapshotStoreBase/ReadAsync"))
        {
            var existing =
                await Collection.Find(x => x.DocumentId.Equals(key))
                    .FirstOrDefaultAsync(ct);

            if (existing != null)
            {
                if (existing.Document is IOnRead onRead)
                {
                    await onRead.OnReadAsync();
                }

                return new SnapshotResult<T>(existing.DocumentId, existing.Document, existing.Version);
            }

            return new SnapshotResult<T>(default, default!, EtagVersion.Empty);
        }
    }

    public async Task WriteAsync(SnapshotWriteJob<T> job,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSnapshotStoreBase/WriteAsync"))
        {
            var entityJob = job.As(CreateDocument(job.Key, job.Value, job.OldVersion));

            await Collection.UpsertVersionedAsync(entityJob, ct);
        }
    }

    public async Task WriteManyAsync(IEnumerable<SnapshotWriteJob<T>> jobs,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSnapshotStoreBase/WriteManyAsync"))
        {
            var writes = jobs.Select(x =>
                new ReplaceOneModel<TState>(Filter.Eq(y => y.DocumentId, x.Key), CreateDocument(x.Key, x.Value, x.NewVersion))
                {
                    IsUpsert = true
                }).ToList();

            if (writes.Count == 0)
            {
                return;
            }

            await Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }
    }

    public async Task RemoveAsync(DomainId key,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSnapshotStoreBase/RemoveAsync"))
        {
            await Collection.DeleteOneAsync(x => x.DocumentId.Equals(key), ct);
        }
    }

    public async IAsyncEnumerable<SnapshotResult<T>> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoSnapshotStoreBase/ReadAllAsync"))
        {
            var find = Collection.Find(FindAll, Batching.Options);

            await foreach (var document in find.ToAsyncEnumerable(ct))
            {
                if (document.Document is IOnRead onRead)
                {
                    await onRead.OnReadAsync();
                }

                yield return new SnapshotResult<T>(document.DocumentId, document.Document, document.Version, true);
            }
        }
    }

    private static TState CreateDocument(DomainId id, T doc, long version)
    {
        var result = new TState
        {
            Document = doc,
            DocumentId = id,
            Version = version
        };

        result.Prepare();

        return result;
    }
}
