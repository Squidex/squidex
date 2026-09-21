// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Extensions.Actions;

internal sealed class ClientPool<TKey, TClient> where TKey : notnull
{
    // The cache is keyed by client configuration, therefore it needs an upper bound to not grow with the number of tenants.
    private const long CacheSizeLimit = 1_000;
    private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(30);
    private readonly MemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions { SizeLimit = CacheSizeLimit }));
    private readonly Func<TKey, Task<TClient>> factory;

    public ClientPool(Func<TKey, TClient> factory)
    {
        this.factory = x => Task.FromResult(factory(x));
    }

    public ClientPool(Func<TKey, Task<TClient>> factory)
    {
        this.factory = factory;
    }

    public async Task<TClient> GetClientAsync(TKey key)
    {
        if (!memoryCache.TryGetValue<TClient>(key, out var client))
        {
            client = await factory(key);

            memoryCache.Set(key, client, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeToLive,

                // The cache has a size limit, therefore every entry must define a size. All entries are counted equally.
                Size = 1,
            });
        }

        return client!;
    }
}
