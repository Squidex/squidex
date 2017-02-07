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
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Utils;
using Squidex.Events;

// ReSharper disable InvertIf

namespace Squidex.Read.Schemas.Services.Implementations
{
    public class CachingSchemaProvider : CachingProvider, ISchemaProvider, ICatchEventConsumer, ILiveEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly ISchemaRepository repository;

        private sealed class CacheItem
        {
            public ISchemaEntityWithSchema Entity;

            public string Name;
        }

        public CachingSchemaProvider(IMemoryCache cache, ISchemaRepository repository)
            : base(cache)
        {
            Guard.NotNull(repository, nameof(repository));

            this.repository = repository;
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaByIdAsync(Guid schemaId)
        {
            var cacheKey = BuildIdCacheKey(schemaId);
            var cacheItem = Cache.Get<CacheItem>(cacheKey);

            if (cacheItem == null)
            {
                var entity = await repository.FindSchemaAsync(schemaId);

                cacheItem = new CacheItem { Entity = entity, Name = entity?.Name };

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

                cacheItem = new CacheItem { Entity = entity, Name = name };

                Cache.Set(cacheKey, cacheItem, CacheDuration);

                if (cacheItem.Entity != null)
                {
                    Cache.Set(BuildIdCacheKey(cacheItem.Entity.Id), cacheItem, CacheDuration);
                }
            }

            return cacheItem.Entity;
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is SchemaDeleted || 
                @event.Payload is SchemaPublished ||
                @event.Payload is SchemaUnpublished ||
                @event.Payload is SchemaUpdated ||
                @event.Payload is FieldEvent)
            {
                var cacheKey = BuildIdCacheKey(@event.Headers.AggregateId());

                var cacheItem = Cache.Get<CacheItem>(cacheKey);

                if (cacheItem?.Name != null)
                {
                    Cache.Remove(BuildNameCacheKey(@event.Headers.AppId(), cacheItem.Name));
                }

                Cache.Remove(cacheKey);
            }
            else
            {
                var schemaCreated = @event.Payload as SchemaCreated;

                if (schemaCreated != null)
                {
                    Cache.Remove(BuildIdCacheKey(@event.Headers.AggregateId()));
                    Cache.Remove(BuildNameCacheKey(@event.Headers.AppId(), schemaCreated.Name));
                }
            }

            return Task.FromResult(true);
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
