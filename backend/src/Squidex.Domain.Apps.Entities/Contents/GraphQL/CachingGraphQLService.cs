﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
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

            var graphQlContext = new GraphQLExecutionContext(context, resolver);

            var result = await Task.WhenAll(queries.Select(q => QueryInternalAsync(model, graphQlContext, q)));

            return (result.Any(x => x.HasError), result.Map(x => x.Response));
        }

        public async Task<(bool HasError, object Response)> QueryAsync(Context context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(query, nameof(query));

            var model = await GetModelAsync(context.App);

            var graphQlContext = new GraphQLExecutionContext(context, resolver);

            var result = await QueryInternalAsync(model, graphQlContext, query);

            return result;
        }

        private static async Task<(bool HasError, object Response)> QueryInternalAsync(GraphQLModel model, GraphQLExecutionContext context, GraphQLQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.Query))
            {
                return (false, new { data = new object() });
            }

            var (data, errors) = await model.ExecuteAsync(context, query);

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
                    resolver.GetRequiredService<GraphQLTypeFactory>(),
                    resolver.GetRequiredService<ISemanticLog>());
            });
        }

        private static object CreateCacheKey(DomainId appId, string etag)
        {
            return $"GraphQLModel_{appId}_{etag}";
        }
    }
}
