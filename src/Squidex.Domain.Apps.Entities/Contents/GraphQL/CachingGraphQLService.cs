// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class CachingGraphQLService : CachingProviderBase, IGraphQLService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IDependencyResolver resolver;

        public CachingGraphQLService(IMemoryCache cache, IDependencyResolver resolver)
            : base(cache)
        {
            Guard.NotNull(resolver, nameof(resolver));

            this.resolver = resolver;
        }

        public async Task<(bool HasError, object Response)> QueryAsync(Context context, params GraphQLQuery[] queries)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(queries, nameof(queries));

            var model = await GetModelAsync(context.App);

            var ctx = new GraphQLExecutionContext(context, resolver);

            var result = await Task.WhenAll(queries.Select(q => QueryInternalAsync(model, ctx, q)));

            return (result.Any(x => x.HasError), result.Map(x => x.Response));
        }

        public async Task<(bool HasError, object Response)> QueryAsync(Context context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(query, nameof(query));

            var model = await GetModelAsync(context.App);

            var ctx = new GraphQLExecutionContext(context, resolver);

            var result = await QueryInternalAsync(model, ctx, query);

            return result;
        }

        private async Task<(bool HasError, object Response)> QueryInternalAsync(GraphQLModel model, GraphQLExecutionContext ctx, GraphQLQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.Query))
            {
                return (false, new { data = new object() });
            }

            var result = await model.ExecuteAsync(ctx, query);

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

                var allSchemas = await resolver.Resolve<IAppProvider>().GetSchemasAsync(app.Id);

                return new GraphQLModel(app,
                    allSchemas,
                    resolver.Resolve<IContentQueryService>().DefaultPageSizeGraphQl,
                    resolver.Resolve<IAssetQueryService>().DefaultPageSizeGraphQl,
                    resolver.Resolve<IGraphQLUrlGenerator>());
            });
        }

        private static object CreateCacheKey(Guid appId, string etag)
        {
            return $"GraphQLModel_{appId}_{etag}";
        }
    }
}
