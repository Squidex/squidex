// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Squidex.Infrastructure.Caching
{
    public sealed class AsyncLocalCache : ILocalCache
    {
        private static readonly AsyncLocal<ConcurrentDictionary<object, object>> Cache = new AsyncLocal<ConcurrentDictionary<object, object>>();
        private static readonly AsyncLocalCleaner Cleaner;

        private sealed class AsyncLocalCleaner : IDisposable
        {
            private readonly AsyncLocal<ConcurrentDictionary<object, object>> cache;

            public AsyncLocalCleaner(AsyncLocal<ConcurrentDictionary<object, object>> cache)
            {
                this.cache = cache;
            }

            public void Dispose()
            {
                cache.Value = null;
            }
        }

        static AsyncLocalCache()
        {
            Cleaner = new AsyncLocalCleaner(Cache);
        }

        public IDisposable StartContext()
        {
            Cache.Value = new ConcurrentDictionary<object, object>();

            return Cleaner;
        }

        public void Add(object key, object value)
        {
            var cacheKey = GetCacheKey(key);

            var cache = Cache.Value;

            if (cache != null)
            {
                cache[cacheKey] = value;
            }
        }

        public void Remove(object key)
        {
            var cacheKey = GetCacheKey(key);

            var cache = Cache.Value;

            if (cache != null)
            {
                cache.TryRemove(cacheKey, out var value);
            }
        }

        public bool TryGetValue(object key, out object value)
        {
            var cacheKey = GetCacheKey(key);

            var cache = Cache.Value;

            if (cache != null)
            {
                return cache.TryGetValue(cacheKey, out value);
            }

            value = null;

            return false;
        }

        private static string GetCacheKey(object key)
        {
            return $"CACHE_{key}";
        }
    }
}
