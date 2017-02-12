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
using Squidex.Infrastructure.Caching;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Utils;

// ReSharper disable InvertIf

namespace Squidex.Read.Apps.Services.Implementations
{
    public class CachingAppProvider : CachingProvider, IAppProvider
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
        private readonly IAppRepository repository;

        public CachingAppProvider(IMemoryCache cache, IAppRepository repository)
            : base(cache)
        {
            Guard.NotNull(cache, nameof(cache));

            this.repository = repository;
        }

        public async Task<IAppEntity> FindAppByIdAsync(Guid appId)
        {
            var cacheKey = BuildIdCacheKey(appId);

            IAppEntity result;
            
            if (!Cache.TryGetValue(cacheKey, out result))
            {
                result = await repository.FindAppAsync(appId);

                Cache.Set(cacheKey, result, CacheDuration);

                if (result != null)
                {
                    Cache.Set(BuildNameCacheKey(result.Name), result, CacheDuration);
                }
            }

            return result;
        }

        public async Task<IAppEntity> FindAppByNameAsync(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BuildNameCacheKey(name);

            IAppEntity result;

            if (!Cache.TryGetValue(cacheKey, out result))
            {
                result = await repository.FindAppAsync(name);

                Cache.Set(cacheKey, result, CacheDuration);

                if (result != null)
                {
                    Cache.Set(BuildIdCacheKey(result.Id), result, CacheDuration);
                }
            }

            return result;
        }

        public void Remove(NamedId<Guid> id)
        {
            var cacheKeyById = BuildIdCacheKey(id.Id);
            var cacheKeyByName = BuildNameCacheKey(id.Name);

            Cache.Remove(cacheKeyById);
            Cache.Remove(cacheKeyByName);

            Cache.Invalidate(cacheKeyById);
            Cache.Invalidate(cacheKeyByName);
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
