// ==========================================================================
//  CachingAppProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Utils;

// ReSharper disable InvertIf

namespace Squidex.Read.Apps.Services.Implementations
{
    public class CachingAppProvider : CachingProvider, IAppProvider
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
        private readonly IAppRepository appRepository;

        private sealed class CacheItem
        {
            public IAppEntity Entity;
        }

        public CachingAppProvider(IMemoryCache cache, IAppRepository appRepository)
            : base(cache)
        {
            Guard.NotNull(cache, nameof(cache));

            this.appRepository = appRepository;
        }

        public async Task<Guid?> FindAppIdByNameAsync(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BulidCacheKey(name);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var app = await appRepository.FindAppByNameAsync(name);

                cacheItem = new CacheItem { Entity = app };

                Cache.Set(cacheKey, cacheItem, new MemoryCacheEntryOptions { SlidingExpiration = CacheDuration });
            }

            return cacheItem.Entity?.Id;
        }

        private static string BulidCacheKey(string name)
        {
            return $"App_{name}";
        }
    }
}
