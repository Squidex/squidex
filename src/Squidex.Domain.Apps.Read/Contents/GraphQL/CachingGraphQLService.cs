// ==========================================================================
//  CachingGraphQLService.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class CachingGraphQLService : CachingProviderBase, IGraphQLService, IEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);
        private readonly IContentQueryService contentQuery;
        private readonly IGraphQLUrlGenerator urlGenerator;
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

        public CachingGraphQLService(IMemoryCache cache,
            IAssetRepository assetRepository,
            IContentQueryService contentQuery,
            IGraphQLUrlGenerator urlGenerator,
            ISchemaRepository schemaRepository)
            : base(cache)
        {
            Guard.NotNull(schemaRepository, nameof(schemaRepository));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentQuery, nameof(urlGenerator));
            Guard.NotNull(contentQuery, nameof(contentQuery));

            this.assetRepository = assetRepository;
            this.contentQuery = contentQuery;
            this.urlGenerator = urlGenerator;
            this.schemaRepository = schemaRepository;
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

        public async Task<(object Data, object[] Errors)> QueryAsync(IAppEntity app, ClaimsPrincipal user, GraphQLQuery query)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            var modelContext = await GetModelAsync(app);
            var queryContext = new QueryContext(app, assetRepository, contentQuery, urlGenerator, user);

            return await modelContext.ExecuteAsync(queryContext, query);
        }

        private async Task<GraphQLModel> GetModelAsync(IAppEntity app)
        {
            var cacheKey = CreateCacheKey(app.Id);

            var modelContext = Cache.Get<GraphQLModel>(cacheKey);

            if (modelContext == null)
            {
                var allSchemas = await schemaRepository.QueryAllAsync(app.Id);

                modelContext = new GraphQLModel(app, allSchemas.Where(x => x.IsPublished), urlGenerator);

                Cache.Set(cacheKey, modelContext, CacheDuration);
            }

            return modelContext;
        }

        private static object CreateCacheKey(Guid appId)
        {
            return $"GraphQLModel_{appId}";
        }
    }
}
