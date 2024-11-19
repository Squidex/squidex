// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;

namespace Squidex.Infrastructure.States;

public abstract class ShardedService<TKey, TService>(IShardingStrategy sharding, Func<string, TService> factory) : IInitializable where TKey : notnull, IDeterministicHashCode
{
    private readonly Dictionary<string, TService> shards = [];

    protected IEnumerable<TService> Shards => shards.Values;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        foreach (var shardKey in sharding.GetShardKeys())
        {
            var inner = factory(shardKey);

            if (inner is IInitializable initializable)
            {
                await initializable.InitializeAsync(ct);
            }

            shards[shardKey] = inner;
        }
    }

    public async Task ReleaseAsync(
        CancellationToken ct)
    {
        foreach (var inner in shards.Values)
        {
            if (inner is IInitializable initializable)
            {
                await initializable.ReleaseAsync(ct);
            }
        }
    }

    protected TService Shard(TKey key)
    {
        var shardKey = sharding.GetShardKey(key);

        return shards[shardKey];
    }
}
