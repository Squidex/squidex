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
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Read.Apps.Services.Implementations
{
    public class CachingAppProvider : CachingProviderBase, IAppProvider, IEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
        private readonly IAppRepository repository;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return string.Empty; }
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

            if (!Cache.TryGetValue(cacheKey, out IAppEntity result))
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

            if (!Cache.TryGetValue(cacheKey, out IAppEntity result))
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

        public Task On(Envelope<IEvent> @event)
        {
            void Remove(NamedId<Guid> id)
            {
                var cacheKeyById = BuildIdCacheKey(id.Id);
                var cacheKeyByName = BuildNameCacheKey(id.Name);

                Cache.Remove(cacheKeyById);
                Cache.Remove(cacheKeyByName);

                Cache.Invalidate(cacheKeyById);
                Cache.Invalidate(cacheKeyByName);
            }

            if (@event.Payload is AppClientAttached ||
                @event.Payload is AppClientChanged ||
                @event.Payload is AppClientRenamed ||
                @event.Payload is AppClientRevoked ||
                @event.Payload is AppPlanChanged ||
                @event.Payload is AppContributorAssigned ||
                @event.Payload is AppContributorRemoved ||
                @event.Payload is AppCreated ||
                @event.Payload is AppLanguageAdded ||
                @event.Payload is AppLanguageRemoved ||
                @event.Payload is AppLanguageUpdated ||
                @event.Payload is AppMasterLanguageSet)
            {
                Remove(((AppEvent)@event.Payload).AppId);
            }

            return TaskHelper.Done;
        }

        private static string BuildNameCacheKey(string name)
        {
            return $"App_Ids_{name}";
        }

        private static string BuildIdCacheKey(Guid schemaId)
        {
            return $"App_Names_{schemaId}";
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }
    }
}
