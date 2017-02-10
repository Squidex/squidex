// ==========================================================================
//  RedisInvalidatingCache.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Squidex.Infrastructure.Redis
{
    public class RedisInvalidatingCache : IMemoryCache
    {
        private readonly IMemoryCache inner;
        private readonly RedisInvalidator invalidator;

        public RedisInvalidatingCache(IMemoryCache inner, IConnectionMultiplexer redis, ILogger<RedisInvalidatingCache> logger)
        {
            Guard.NotNull(redis, nameof(redis));
            Guard.NotNull(inner, nameof(inner));
            Guard.NotNull(logger, nameof(logger));

            this.inner = inner;

            invalidator = new RedisInvalidator(redis, inner, logger);
        }

        public void Dispose()
        {
            inner.Dispose();
        }

        public bool TryGetValue(object key, out object value)
        {
            return inner.TryGetValue(key, out value);
        }

        public void Remove(object key)
        {
            inner.Remove(key);

            if (key is string)
            {
                invalidator.Invalidate(key.ToString());
            }
        }

        public ICacheEntry CreateEntry(object key)
        {
            return new WrapperCacheEntry(inner.CreateEntry(key), invalidator);
        }
    }
}
