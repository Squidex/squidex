// ==========================================================================
//  CachedGraphQLInvoker.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Domain.Apps.Read.Utils;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Infrastructure.CQRS.Events;
using System;
using Squidex.Infrastructure.Tasks;
using Squidex.Domain.Apps.Events;

// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class CachingGraphQLInvoker : CachingProviderBase, IGraphQLInvoker, IEventConsumer
    {
        private readonly IContentRepository contentRepository;
        private readonly IAssetRepository assetRepository;
        private readonly ISchemaRepository schemaRepository;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^(schema-)|(apps-)"; }
        }

        public CachingGraphQLInvoker(IMemoryCache cache, ISchemaRepository schemaRepository, IAssetRepository assetRepository, IContentRepository contentRepository)
            : base(cache)
        {
            Guard.NotNull(schemaRepository, nameof(schemaRepository));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.schemaRepository = schemaRepository;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is AppEvent appEvent)
            {
                Cache.Remove(CreateCacheKey(appEvent.AppId.Id));
            }

            return TaskHelper.Done;
        }

        public async Task<object> QueryAsync(IAppEntity appEntity, GraphQLQuery query)
        {
            Guard.NotNull(appEntity, nameof(appEntity));
            Guard.NotNull(query, nameof(query));

            var modelContext = await GetModelAsync(appEntity);
            var queryContext = new QueryContext(appEntity, contentRepository, assetRepository);

            return await modelContext.ExecuteAsync(queryContext, query);
        }

        private async Task<GraphQLModel> GetModelAsync(IAppEntity appEntity)
        {
            var cacheKey = CreateCacheKey(appEntity.Id);

            var modelContext = Cache.Get<GraphQLModel>(cacheKey);

            if (modelContext == null)
            {
                var schemas = await schemaRepository.QueryAllAsync(appEntity.Id);

                modelContext = new GraphQLModel(appEntity, schemas.Where(x => x.IsPublished));

                Cache.Set(cacheKey, modelContext);
            }

            return modelContext;
        }

        private static object CreateCacheKey(Guid appId)
        {
            return $"GraphQLModel_{appId}";
        }
    }
}
