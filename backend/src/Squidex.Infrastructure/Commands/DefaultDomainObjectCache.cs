﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Infrastructure.Commands;

public sealed class DefaultDomainObjectCache : IDomainObjectCache
{
    private readonly DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions();
    private readonly IMemoryCache cache;
    private readonly IJsonSerializer serializer;
    private readonly IDistributedCache distributedCache;

    public DefaultDomainObjectCache(IMemoryCache cache, IJsonSerializer serializer, IDistributedCache distributedCache,
        IOptions<DomainObjectCacheOptions> options)
    {
        this.cache = cache;
        this.serializer = serializer;
        this.distributedCache = distributedCache;

        if (options.Value.CacheDuration > TimeSpan.Zero)
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = options.Value.CacheDuration;
        }
    }

    public async Task<T> GetAsync<T>(DomainId id, long version,
        CancellationToken ct = default)
    {
        if (cacheOptions.AbsoluteExpirationRelativeToNow == null)
        {
            return default!;
        }

        var cacheKey = CacheKey(id, version);

        if (cache.TryGetValue(cacheKey, out var found) && found is T typed)
        {
            return typed;
        }

        var cached = await distributedCache.GetAsync(cacheKey, ct);
        if (cached == null)
        {
            return default!;
        }

        try
        {
            using var stream = new MemoryStream(cached);
            return serializer.Deserialize<T>(stream);
        }
        catch
        {
            return default!;
        }
    }

    public async Task SetAsync<T>(DomainId id, long version, T snapshot,
        CancellationToken ct = default)
    {
        if (cacheOptions.AbsoluteExpirationRelativeToNow == null)
        {
            return;
        }

        var cacheKey = CacheKey(id, version);

        cache.Set(cacheKey, snapshot, cacheOptions.AbsoluteExpirationRelativeToNow!.Value);
        try
        {
            using var stream = DefaultPools.MemoryStream.GetStream();
            serializer.Serialize(snapshot, stream);

            await distributedCache.SetAsync(cacheKey, stream.ToArray(), cacheOptions, ct);
        }
        catch
        {
            return;
        }
    }

    private static string CacheKey(DomainId key, long version)
    {
        return $"{key}_{version}";
    }
}
