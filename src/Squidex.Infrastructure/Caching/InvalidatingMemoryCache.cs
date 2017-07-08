// ==========================================================================
//  InvalidatingMemoryCache.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure.Caching
{
    public class InvalidatingMemoryCache : IMemoryCache, IInvalidatingCache
    {
        private const string ChannelName = "CacheInvalidations";
        private readonly IMemoryCache inner;
        private readonly IPubSub invalidator;

        public InvalidatingMemoryCache(IMemoryCache inner, IPubSub invalidator)
        {
            Guard.NotNull(inner, nameof(inner));
            Guard.NotNull(invalidator, nameof(invalidator));

            this.inner = inner;
            this.invalidator = invalidator;

            invalidator.Subscribe(ChannelName, inner.Remove);
        }

        public void Dispose()
        {
            inner.Dispose();
        }

        public ICacheEntry CreateEntry(object key)
        {
            return inner.CreateEntry(key);
        }

        public bool TryGetValue(object key, out object value)
        {
            return inner.TryGetValue(key, out value);
        }

        public void Remove(object key)
        {
            inner.Remove(key);
        }

        public void Invalidate(object key)
        {
            if (key is string)
            {
                invalidator.Publish(ChannelName, key.ToString(), true);
            }
        }
    }
}
