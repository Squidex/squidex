// ==========================================================================
//  CachingSchemaProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PinkParrot.Events.Schemas;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Read.Schemas.Repositories;

// ReSharper disable InvertIf

namespace PinkParrot.Read.Schemas.Services.Implementations
{
    public class CachingSchemaProvider : ISchemaProvider, ILiveEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IMemoryCache cache;
        private readonly ISchemaRepository repository;

        public sealed class CacheItem
        {
            public ISchemaEntityWithSchema Entity;
        }

        public CachingSchemaProvider(IMemoryCache cache, ISchemaRepository repository)
        {
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(repository, nameof(repository));

            this.cache = cache;

            this.repository = repository;
        }

        public async Task<Guid?> FindSchemaIdByNameAsync(Guid tenantId, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BuildModelsCacheKey(tenantId, name);
            var cacheItem = cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var schema = await repository.FindSchemaAsync(tenantId, name);

                cacheItem = new CacheItem { Entity = schema };

                cache.Set(cacheKey, cacheItem, CacheDuration);

                if (cacheItem.Entity != null)
                {
                    cache.Set(BuildNamesCacheKey(cacheItem.Entity.Id), cacheItem.Entity.Name, CacheDuration);
                }
            }

            return cacheItem?.Entity?.Id;
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is SchemaUpdated || @event.Payload is SchemaDeleted)
            {
                var oldName = cache.Get<string>(BuildNamesCacheKey(@event.Headers.AggregateId()));

                if (oldName != null)
                {
                    cache.Remove(BuildModelsCacheKey(@event.Headers.TenantId(), oldName));
                }
            }

            return Task.FromResult(true);
        }

        private static string BuildModelsCacheKey(Guid tenantId, string name)
        {
            return $"Schemas_Models_{tenantId}_{name}";
        }

        private static string BuildNamesCacheKey(Guid schemaId)
        {
            return $"Schema_Names_{schemaId}";
        }
    }
}
