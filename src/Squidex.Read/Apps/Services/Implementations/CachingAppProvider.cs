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
        private readonly IAppRepository repository;

        private sealed class CacheItem
        {
            public IAppEntity Entity;

            public string Name;
        }

        public CachingAppProvider(IMemoryCache cache, IAppRepository repository)
            : base(cache)
        {
            Guard.NotNull(cache, nameof(cache));

            this.repository = repository;
        }

        public async Task<IAppEntity> FindAppByIdAsync(Guid appId)
        {
            var cacheKey = BuildIdCacheKey(appId);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var entity = await repository.FindAppAsync(appId);

                cacheItem = new CacheItem { Entity = entity, Name = entity.Name };

                Cache.Set(cacheKey, cacheItem, CacheDuration);

                if (cacheItem.Entity != null)
                {
                    Cache.Set(BuildNameCacheKey(cacheItem.Name), cacheItem, CacheDuration);
                }
            }

            return cacheItem.Entity;
        }

        public async Task<IAppEntity> FindAppByNameAsync(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BuildNameCacheKey(name);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var entity = await repository.FindAppAsync(name);

                cacheItem = new CacheItem { Entity = entity, Name = name };

                Cache.Set(cacheKey, cacheItem, CacheDuration);

                if (cacheItem.Entity != null)
                {
                    Cache.Set(BuildIdCacheKey(cacheItem.Entity.Id), cacheItem, CacheDuration);
                }
            }

            return cacheItem.Entity;
        }

        public void Remove(Guid id)
        {
            var cacheKey = BuildIdCacheKey(id);

            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem?.Name != null)
            {
                Cache.Remove(BuildNameCacheKey(cacheItem.Name));
            }

            Cache.Remove(cacheKey);
        }

        private static string BuildNameCacheKey(string name)
        {
            return $"App_Ids_{name}";
        }

        private static string BuildIdCacheKey(Guid schemaId)
        {
            return $"App_Names_{schemaId}";
        }
    }
}
