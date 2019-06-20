// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class CachingGraphQLService : CachingProviderBase, IGraphQLService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IContentQueryService contentQuery;
        private readonly IGraphQLUrlGenerator urlGenerator;
        private readonly ISemanticLog log;
        private readonly IAssetQueryService assetQuery;
        private readonly IAppProvider appProvider;

        public CachingGraphQLService(
            IMemoryCache cache,
            IAppProvider appProvider,
            IAssetQueryService assetQuery,
            IContentQueryService contentQuery,
            IGraphQLUrlGenerator urlGenerator,
            ISemanticLog log)
            : base(cache)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));
            Guard.NotNull(log, nameof(log));

            this.appProvider = appProvider;
            this.assetQuery = assetQuery;
            this.contentQuery = contentQuery;
            this.urlGenerator = urlGenerator;
            this.log = log;
        }

        public async Task<(bool HasError, object Response)> QueryAsync(QueryContext context, params GraphQLQuery[] queries)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(queries, nameof(queries));

            var model = await GetModelAsync(context.App);

            var ctx = new GraphQLExecutionContext(context, assetQuery, contentQuery, urlGenerator);

            var result = await Task.WhenAll(queries.Select(q => QueryInternalAsync(model, ctx, q)));

            return (result.Any(x => x.HasError), result.ToArray(x => x.Response));
        }

        public async Task<(bool HasError, object Response)> QueryAsync(QueryContext context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(query, nameof(query));

            var model = await GetModelAsync(context.App);

            var ctx = new GraphQLExecutionContext(context, assetQuery, contentQuery, urlGenerator);

            var result = await QueryInternalAsync(model, ctx, query);

            return result;
        }

        private async Task<(bool HasError, object Response)> QueryInternalAsync(GraphQLModel model, GraphQLExecutionContext ctx, GraphQLQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.Query))
            {
                return (false, new { data = new object() });
            }

            var result = await model.ExecuteAsync(ctx, query, log);

            if (result.Errors?.Any() == true)
            {
                return (false, new { data = result.Data, errors = result.Errors });
            }
            else
            {
                return (false, new { data = result.Data });
            }
        }

        private Task<GraphQLModel> GetModelAsync(IAppEntity app)
        {
            var cacheKey = CreateCacheKey(app.Id, app.Version.ToString());

            return Cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                var allSchemas = await appProvider.GetSchemasAsync(app.Id);

                return new GraphQLModel(app, allSchemas, contentQuery.DefaultPageSizeGraphQl, assetQuery.DefaultPageSizeGraphQl, urlGenerator);
            });
        }

        private static object CreateCacheKey(Guid appId, string etag)
        {
            return $"GraphQLModel_{appId}_{etag}";
        }
    }
}
