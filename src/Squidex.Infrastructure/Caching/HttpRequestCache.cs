// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;

namespace Squidex.Infrastructure.Caching
{
    public sealed class HttpRequestCache : IRequestCache
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpRequestCache(IHttpContextAccessor httpContextAccessor)
        {
            Guard.NotNull(httpContextAccessor, nameof(httpContextAccessor));

            this.httpContextAccessor = httpContextAccessor;
        }

        public void Add(object key, object value)
        {
            var cacheKey = GetCacheKey(key);

            var items = httpContextAccessor.HttpContext?.Items;

            if (items != null)
            {
                items[cacheKey] = value;
            }
        }

        public void Remove(object key)
        {
            var cacheKey = GetCacheKey(key);

            var items = httpContextAccessor.HttpContext?.Items;

            if (items != null)
            {
                items?.Remove(cacheKey);
            }
        }

        public bool TryGetValue(object key, out object value)
        {
            var cacheKey = GetCacheKey(key);

            var items = httpContextAccessor.HttpContext?.Items;

            if (items != null)
            {
                return items.TryGetValue(cacheKey, out value);
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
