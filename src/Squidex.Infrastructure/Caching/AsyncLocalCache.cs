// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Caching
{
    public sealed class AsyncLocalCache : ILocalCache
    {
        private static readonly AsyncLocal<ConcurrentDictionary<object, object>> LocalCache = new AsyncLocal<ConcurrentDictionary<object, object>>();
        private static readonly AsyncLocalCleaner<ConcurrentDictionary<object, object>> Cleaner;

        static AsyncLocalCache()
        {
            Cleaner = new AsyncLocalCleaner<ConcurrentDictionary<object, object>>(LocalCache);
        }

        public IDisposable StartContext()
        {
            LocalCache.Value = new ConcurrentDictionary<object, object>();

            return Cleaner;
        }

        public void Add(object key, object value)
        {
            var cacheKey = GetCacheKey(key);

            var cache = LocalCache.Value;

            if (cache != null)
            {
                cache[cacheKey] = value;
            }
        }

        public void Remove(object key)
        {
            var cacheKey = GetCacheKey(key);

            var cache = LocalCache.Value;

            if (cache != null)
            {
                cache.TryRemove(cacheKey, out _);
            }
        }

        public bool TryGetValue(object key, out object value)
        {
            var cacheKey = GetCacheKey(key);

            var cache = LocalCache.Value;

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
