// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Namotion.Reflection;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb;

public abstract class ShardedSnapshotStore<T, TState> : ShardedService<T>, ISnapshotStore<TState>, IDeleter where T : ISnapshotStore<TState>, IDeleter
{
    protected ShardedSnapshotStore(IShardingStrategy sharding, Func<string, T> factory)
        : base(sharding, factory)
    {
    }

    protected abstract string GetShardKey(TState state);

    public Task WriteAsync(SnapshotWriteJob<TState> job,
        CancellationToken ct = default)
    {
        var shard = Shard(GetShardKey(job.Value));

        return shard.WriteAsync(job, ct);
    }

    public Task<SnapshotResult<TState>> ReadAsync(DomainId key,
        CancellationToken ct = default)
    {
        var shard = Shard(GetAppId(key));

        return shard.ReadAsync(key, ct);
    }

    public Task RemoveAsync(DomainId key,
        CancellationToken ct = default)
    {
        var shard = Shard(GetAppId(key));

        return shard.ReadAsync(key, ct);
    }

    public Task DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        var shard = Shard(app.Id);

        return shard.DeleteAppAsync(app, ct);
    }

    public async IAsyncEnumerable<SnapshotResult<TState>> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var shard in Shards)
        {
            await foreach (var asset in shard.ReadAllAsync(ct))
            {
                yield return asset;
            }
        }
    }

    public async Task ClearAsync(
        CancellationToken ct = default)
    {
        foreach (var shard in Shards)
        {
            await shard.ClearAsync(ct);
        }
    }

    public async Task WriteManyAsync(IEnumerable<SnapshotWriteJob<TState>> jobs,
        CancellationToken ct = default)
    {
        // Some commands might share a shared, therefore we don't group by app id.
        foreach (var byShard in jobs.GroupBy(c => GetShardKey(c.Value)))
        {
            var shard = Shard(byShard.Key);

            await shard.WriteManyAsync(byShard.ToArray(), ct);
        }
    }

    private static DomainId GetAppId(DomainId key)
    {
        // This is a leaky abstraction, but the only option to implement that in a fast way.
        var parts = key.ToString().Split(DomainId.IdSeparator);

        if (parts.Length != 2)
        {
            throw new InvalidOperationException("The key does not contain an app id.");
        }

        return DomainId.Create(parts[0]);
    }
}
