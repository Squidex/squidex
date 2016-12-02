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
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Utils;

// ReSharper disable InvertIf

namespace Squidex.Read.Apps.Services.Implementations
{
    public class CachingAppProvider : CachingProvider, IAppProvider, ILiveEventConsumer
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

        public async Task<IAppEntity> FindAppByNameAsync(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BuildModelCacheKey(name);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var app = await appRepository.FindAppByNameAsync(name);

                cacheItem = new CacheItem { Entity = app };

                Cache.Set(cacheKey, cacheItem, new MemoryCacheEntryOptions { SlidingExpiration = CacheDuration });

                if (cacheItem.Entity != null)
                {
                    Cache.Set(BuildNamesCacheKey(cacheItem.Entity.Id), cacheItem.Entity.Name, CacheDuration);
                }
            }

            return cacheItem.Entity;
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is AppContributorAssigned || 
                @event.Payload is AppContributorRemoved ||
                @event.Payload is AppClientAttached || 
                @event.Payload is AppClientRevoked ||
                @event.Payload is AppClientRenamed ||
                @event.Payload is AppLanguagesConfigured)
            {
                var appName = Cache.Get<string>(BuildNamesCacheKey(@event.Headers.AggregateId()));

                if (appName != null)
                {
                    Cache.Remove(BuildModelCacheKey(appName));
                }
            }

            return Task.FromResult(true);
        }

        private static string BuildNamesCacheKey(Guid schemaId)
        {
            return $"App_Names_{schemaId}";
        }

        private static string BuildModelCacheKey(string name)
        {
            return $"App_{name}";
        }
    }
}
