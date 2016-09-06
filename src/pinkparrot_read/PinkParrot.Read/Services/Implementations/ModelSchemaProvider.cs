// ==========================================================================
//  ModelSchemaProvider.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PinkParrot.Events.Schema;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS;
using PinkParrot.Infrastructure.CQRS.Events;
using PinkParrot.Read.Repositories;
// ReSharper disable InvertIf

namespace PinkParrot.Read.Services.Implementations
{
    public class ModelSchemaProvider : IModelSchemaProvider, ILiveEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IMemoryCache cache;
        private readonly IModelSchemaRepository repository;

        public ModelSchemaProvider(IMemoryCache cache, IModelSchemaRepository repository)
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
            var cacheItem = cache.Get<EntityWithSchema>(cacheKey);

            if (cacheItem == null)
            {
                cacheItem = await repository.FindSchemaAsync(tenantId, name) ?? new EntityWithSchema(null, null);

                cache.Set(cacheKey, cacheItem, CacheDuration);

                if (cacheItem.Entity != null)
                {
                    cache.Set(BuildNamesCacheKey(cacheItem.Entity.Id), cacheItem.Entity.Name, CacheDuration);
                }
            }

            return cacheItem.Entity?.Id;
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is ModelSchemaUpdated || @event.Payload is ModelSchemaDeleted)
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
