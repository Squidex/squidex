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
using PinkParrot.Read.Utils;

// ReSharper disable InvertIf

namespace PinkParrot.Read.Schemas.Services.Implementations
{
    public class CachingSchemaProvider : CachingProvider, ISchemaProvider, ILiveEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly ISchemaRepository repository;

        private sealed class CacheItem
        {
            public ISchemaEntityWithSchema Entity;
        }

        public CachingSchemaProvider(IMemoryCache cache, ISchemaRepository repository)
            : base(cache)
        {
            Guard.NotNull(repository, nameof(repository));

            this.repository = repository;
        }

        public async Task<Guid?> FindSchemaIdByNameAsync(Guid appId, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var cacheKey = BuildModelsCacheKey(appId, name);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var schema = await repository.FindSchemaAsync(appId, name);

                cacheItem = new CacheItem { Entity = schema };

                Cache.Set(cacheKey, cacheItem, CacheDuration);

                if (cacheItem.Entity != null)
                {
                    Cache.Set(BuildNamesCacheKey(cacheItem.Entity.Id), cacheItem.Entity.Name, CacheDuration);
                }
            }

            return cacheItem.Entity?.Id;
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is SchemaUpdated || @event.Payload is SchemaDeleted)
            {
                var oldName = Cache.Get<string>(BuildNamesCacheKey(@event.Headers.AggregateId()));

                if (oldName != null)
                {
                    Cache.Remove(BuildModelsCacheKey(@event.Headers.AppId(), oldName));
                }
            }

            return Task.FromResult(true);
        }

        private static string BuildModelsCacheKey(Guid appId, string name)
        {
            return $"Schemas_Models_{appId}_{name}";
        }

        private static string BuildNamesCacheKey(Guid schemaId)
        {
            return $"Schema_Names_{schemaId}";
        }
    }
}
