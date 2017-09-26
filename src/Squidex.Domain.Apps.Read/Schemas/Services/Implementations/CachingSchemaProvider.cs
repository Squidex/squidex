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
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Schemas.Services.Implementations
{
    public class CachingSchemaProvider : CachingProviderBase, ISchemaProvider, IEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly ISchemaRepository repository;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return string.Empty; }
        }

        public CachingSchemaProvider(IMemoryCache cache, ISchemaRepository repository)
            : base(cache)
        {
            Guard.NotNull(repository, nameof(repository));

            this.repository = repository;
        }

        public async Task<ISchemaEntity> FindSchemaByIdAsync(Guid id, bool provideDeleted = false)
        {
            var cacheKey = BuildIdCacheKey(id);

            if (!Cache.TryGetValue(cacheKey, out ISchemaEntity result))
            {
                result = await repository.FindSchemaAsync(id);

                Cache.Set(cacheKey, result, CacheDuration);

                if (result != null)
                {
                    Cache.Set(BuildNameCacheKey(result.AppId, result.Name), result, CacheDuration);
                }
            }

            if (result != null && result.IsDeleted && !provideDeleted)
            {
                result = null;
            }

            return result;
        }

        public async Task<ISchemaEntity> FindSchemaByNameAsync(Guid appId, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BuildNameCacheKey(appId, name);

            if (!Cache.TryGetValue(cacheKey, out ISchemaEntity result))
            {
                result = await repository.FindSchemaAsync(appId, name);

                Cache.Set(cacheKey, result, CacheDuration);

                if (result != null)
                {
                    Cache.Set(BuildIdCacheKey(result.Id), result, CacheDuration);
                }
            }

            if (result != null && result.IsDeleted)
            {
                result = null;
            }

            return result;
        }

        public Task On(Envelope<IEvent> @event)
        {
            void Remove(NamedId<Guid> appId, NamedId<Guid> schemaId)
            {
                var cacheKeyById = BuildIdCacheKey(schemaId.Id);
                var cacheKeyByName = BuildNameCacheKey(appId.Id, schemaId.Name);

                Cache.Remove(cacheKeyById);
                Cache.Remove(cacheKeyByName);

                Cache.Invalidate(cacheKeyById);
                Cache.Invalidate(cacheKeyByName);
            }

            if (@event.Payload is SchemaEvent schemaEvent)
            {
                Remove(schemaEvent.AppId, schemaEvent.SchemaId);
            }

            return TaskHelper.Done;
        }

        private static string BuildNameCacheKey(Guid appId, string name)
        {
            return $"Schema_Ids_{appId}_{name}";
        }

        private static string BuildIdCacheKey(Guid schemaId)
        {
            return $"Schema_Names_{schemaId}";
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }
    }
}
