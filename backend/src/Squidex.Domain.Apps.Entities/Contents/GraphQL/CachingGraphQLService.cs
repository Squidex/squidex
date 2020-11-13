// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class CachingGraphQLService : IGraphQLService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IMemoryCache cache;
        private readonly IServiceProvider resolver;

        public CachingGraphQLService(IMemoryCache cache, IServiceProvider resolver)
        {
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(resolver, nameof(resolver));

            this.cache = cache;
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

        private static async Task<(bool HasError, object Response)> QueryInternalAsync(GraphQLModel model, GraphQLExecutionContext ctx, GraphQLQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.Query))
            {
                return (false, new { data = new object() });
            }

            var (data, errors) = await model.ExecuteAsync(ctx, query);

            if (errors?.Any() == true)
            {
                return (false, new { data, errors });
            }
            else
            {
                return (false, new { data });
            }
        }

        private Task<GraphQLModel> GetModelAsync(IAppEntity app)
        {
            var cacheKey = CreateCacheKey(app.Id, app.Version.ToString());

            return cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                var allSchemas = await resolver.GetRequiredService<IAppProvider>().GetSchemasAsync(app.Id);

                return new GraphQLModel(app,
                    allSchemas,
                    GetPageSizeForContents(),
                    GetPageSizeForAssets(),
                    resolver.GetRequiredService<IUrlGenerator>(),
                    resolver.GetRequiredService<ISemanticLog>());
            });
        }

        private int GetPageSizeForContents()
        {
            return resolver.GetRequiredService<IOptions<ContentOptions>>().Value.DefaultPageSizeGraphQl;
        }

        private int GetPageSizeForAssets()
        {
            return resolver.GetRequiredService<IOptions<AssetOptions>>().Value.DefaultPageSizeGraphQl;
        }

        private static object CreateCacheKey(DomainId appId, string etag)
        {
            return $"GraphQLModel_{appId}_{etag}";
        }
    }
}
