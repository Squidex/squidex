// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Infrastructure.Commands
{
    public sealed class DefaultDomainObjectCache : IDomainObjectCache
    {
        private static readonly DistributedCacheEntryOptions CacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        private readonly IMemoryCache cache;
        private readonly IJsonSerializer serializer;
        private readonly IDistributedCache distributedCache;

        public DefaultDomainObjectCache(IMemoryCache cache, IJsonSerializer serializer, IDistributedCache distributedCache)
        {
            this.cache = cache;
            this.serializer = serializer;
            this.distributedCache = distributedCache;
        }

        public async Task<T> GetAsync<T>(DomainId id, long version,
            CancellationToken ct = default)
        {
            var cacheKey = CacheKey(id, version);

            if (cache.TryGetValue(cacheKey, out var found) && found is T typed)
            {
                return typed;
            }

            var buffer = await distributedCache.GetAsync(cacheKey, ct);

            if (buffer == null)
            {
                return default!;
            }

            try
            {
                using (var stream = new MemoryStream(buffer))
                {
                    var result = serializer.Deserialize<T>(stream);

                    return result;
                }
            }
            catch
            {
                return default!;
            }
        }

        public async Task SetAsync<T>(DomainId id, long version, T snapshot,
            CancellationToken ct = default)
        {
            var cacheKey = CacheKey(id, version);

            cache.Set(cacheKey, snapshot, CacheOptions.SlidingExpiration!.Value);

            try
            {
                using (var stream = DefaultPools.MemoryStream.GetStream())
                {
                    serializer.Serialize<T>(snapshot, stream, true);

                    await distributedCache.SetAsync(cacheKey, stream.ToArray(), CacheOptions, ct);
                }
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
}
