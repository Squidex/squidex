// ==========================================================================
//  CachingSchemaProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Utils;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Read.Schemas.Services.Implementations
{
    public class CachingSchemaProvider : CachingProvider, ISchemaProvider
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly ISchemaRepository repository;

        private sealed class CacheItem
        {
            public ISchemaEntityWithSchema Entity;

            public Guid AppId;

            public string Name;
        }

        public CachingSchemaProvider(IMemoryCache cache, ISchemaRepository repository)
            : base(cache)
        {
            Guard.NotNull(repository, nameof(repository));

            this.repository = repository;
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaByIdAsync(Guid id)
        {
            var cacheKey = BuildIdCacheKey(id);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var entity = await repository.FindSchemaAsync(id);

                if (entity == null)
                {
                    cacheItem = new CacheItem();
                }
                else
                {
                    cacheItem = new CacheItem { Entity = entity, Name = entity.Name, AppId = entity.AppId };
                }

                Cache.Set(cacheKey, cacheItem, CacheDuration);

                if (cacheItem.Entity != null)
                {
                    Cache.Set(BuildNameCacheKey(cacheItem.Entity.AppId, cacheItem.Entity.Name), cacheItem, CacheDuration);
                }
            }

            return cacheItem.Entity;
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaByNameAsync(Guid appId, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BuildNameCacheKey(appId, name);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var entity = await repository.FindSchemaAsync(appId, name);

                cacheItem = new CacheItem { Entity = entity, Name = name, AppId = appId };

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
                Cache.Remove(BuildNameCacheKey(cacheItem.AppId, cacheItem.Name));
            }

            Cache.Remove(cacheKey);
        }

        private static string BuildNameCacheKey(Guid appId, string name)
        {
            return $"Schema_Ids_{appId}_{name}";
        }

        private static string BuildIdCacheKey(Guid schemaId)
        {
            return $"Schema_Names_{schemaId}";
        }
    }
}
