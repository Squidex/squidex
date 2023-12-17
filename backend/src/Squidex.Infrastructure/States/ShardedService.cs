// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;

namespace Squidex.Infrastructure.States;

public abstract class ShardedService<T> : IInitializable
{
    private readonly Dictionary<string, T> shards = new Dictionary<string, T>();
    private readonly IShardingStrategy sharding;
    private readonly Func<string, T> factory;

    protected IEnumerable<T> Shards => shards.Values;

    protected ShardedService(IShardingStrategy sharding, Func<string, T> factory)
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

    protected string GetShardKey<TKey>(TKey key) where TKey : notnull
    {
        return sharding.GetShardKey(key);
    }

    protected T Shard<TKey>(TKey key) where TKey : notnull
    {
        return shards[GetShardKey(key)];
    }

    protected string GetShardKey(DomainId appId)
    {
        return sharding.GetShardKey(appId);
    }
}
