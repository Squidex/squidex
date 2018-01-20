// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class CachingGraphQLService : CachingProviderBase, IGraphQLService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IContentQueryService contentQuery;
        private readonly ICommandBus commandBus;
        private readonly IGraphQLUrlGenerator urlGenerator;
        private readonly IAssetRepository assetRepository;
        private readonly IAppProvider appProvider;

        public CachingGraphQLService(IMemoryCache cache,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            ICommandBus commandBus,
            IContentQueryService contentQuery,
            IGraphQLUrlGenerator urlGenerator)
            : base(cache)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(commandBus, nameof(commandBus));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.appProvider = appProvider;
            this.assetRepository = assetRepository;
            this.commandBus = commandBus;
            this.contentQuery = contentQuery;
            this.urlGenerator = urlGenerator;
        }

        public async Task<(object Data, object[] Errors)> QueryAsync(IAppEntity app, ClaimsPrincipal user, GraphQLQuery query)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            if (string.IsNullOrWhiteSpace(query.Query))
            {
                return (new object(), new object[0]);
            }

            var modelContext = await GetModelAsync(app);

            var ctx = new GraphQLExecutionContext(app, assetRepository, commandBus, contentQuery, user, urlGenerator);

            return await modelContext.ExecuteAsync(ctx, query);
        }

        private async Task<GraphQLModel> GetModelAsync(IAppEntity app)
        {
            var cacheKey = CreateCacheKey(app.Id, app.Version.ToString());

            var modelContext = Cache.Get<GraphQLModel>(cacheKey);

            if (modelContext == null)
            {
                var allSchemas = await appProvider.GetSchemasAsync(app.Id);

                modelContext = new GraphQLModel(app, allSchemas.Where(x => x.IsPublished), urlGenerator);

                Cache.Set(cacheKey, modelContext, CacheDuration);
            }

            return modelContext;
        }

        private static object CreateCacheKey(Guid appId, string etag)
        {
            return $"GraphQLModel_{appId}_{etag}";
        }
    }
}
