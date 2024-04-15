// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;

namespace Squidex.Infrastructure.States;

public abstract class ShardedService<TKey, TService> : IInitializable where TKey : notnull, IDeterministicHashCode
{
    private readonly Dictionary<string, TService> shards = [];
    private readonly IShardingStrategy sharding;
    private readonly Func<string, TService> factory;

    protected IEnumerable<TService> Shards => shards.Values;

    protected ShardedService(IShardingStrategy sharding, Func<string, TService> factory)
    {
        this.sharding = sharding;
        this.factory = factory;
    }

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
