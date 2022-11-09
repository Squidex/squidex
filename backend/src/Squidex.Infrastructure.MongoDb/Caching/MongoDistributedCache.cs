// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.Caching;

public sealed class MongoDistributedCache : MongoRepositoryBase<MongoCacheEntry>, IDistributedCache
{
    public MongoDistributedCache(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Cache";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoCacheEntry> collection,
        CancellationToken ct)
    {
        return Collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoCacheEntry>(
                Index.Ascending(x => x.Expires),
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.Zero
                }),
            null, ct);
    }

    public byte[] Get(string key)
    {
        throw new NotSupportedException();
    }

    public void Refresh(string key)
    {
        throw new NotSupportedException();
    }

    public void Remove(string key)
    {
        throw new NotSupportedException();
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        throw new NotSupportedException();
    }

    public Task RefreshAsync(string key,
        CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key,
        CancellationToken token = default)
    {
        return Collection.DeleteOneAsync(x => x.Key == key, token);
    }

    public async Task<byte[]?> GetAsync(string key,
        CancellationToken token = default)
    {
        var entry = await Collection.Find(x => x.Key == key).FirstOrDefaultAsync(token);

        if (entry != null && entry.Expires > DateTime.UtcNow)
        {
            return entry.Value;
        }

        return null;
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        var expires = DateTime.UtcNow;

        if (options.AbsoluteExpiration.HasValue)
        {
            expires = options.AbsoluteExpiration.Value.UtcDateTime;
        }
        else if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            expires += options.AbsoluteExpirationRelativeToNow.Value;
        }
        else if (options.SlidingExpiration.HasValue)
        {
            expires += options.SlidingExpiration.Value;
        }

        return Collection.UpdateOneAsync(x => x.Key == key,
            Update
                .Set(x => x.Value, value)
                .Set(x => x.Expires, expires),
            Upsert, token);
    }
}
