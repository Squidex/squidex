// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Log;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class CachingGraphQLService : IGraphQLService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IBackgroundCache cache;
        private readonly ISchemasHash schemasHash;
        private readonly IServiceProvider serviceProvider;
        private readonly GraphQLOptions options;

        public sealed record CacheEntry(GraphQLModel Model, string Hash, Instant Created);

        public CachingGraphQLService(IBackgroundCache cache, ISchemasHash schemasHash, IServiceProvider serviceProvider, IOptions<GraphQLOptions> options)
        {
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(schemasHash, nameof(schemasHash));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(options, nameof(options));

            this.cache = cache;
            this.schemasHash = schemasHash;
            this.serviceProvider = serviceProvider;
            this.options = options.Value;
        }

        public async Task<(bool HasError, object Response)> QueryAsync(Context context, params GraphQLQuery[] queries)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(queries, nameof(queries));

            var model = await GetModelAsync(context.App);

            var executionContext =
                serviceProvider.GetRequiredService<GraphQLExecutionContext>()
                    .WithContext(context);

            var result = await Task.WhenAll(queries.Select(q => QueryInternalAsync(model, executionContext, q)));

            return (result.Any(x => x.HasError), result.Select(x => x.Response).ToArray());
        }

        public async Task<(bool HasError, object Response)> QueryAsync(Context context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(query, nameof(query));

            var model = await GetModelAsync(context.App);

            var executionContext =
                serviceProvider.GetRequiredService<GraphQLExecutionContext>()
                    .WithContext(context);

            var result = await QueryInternalAsync(model, executionContext, query);

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

        private async Task<GraphQLModel> GetModelAsync(IAppEntity app)
        {
            var entry = await GetModelEntryAsync(app);

            return entry.Model;
        }

        private Task<CacheEntry> GetModelEntryAsync(IAppEntity app)
        {
            if (options.CacheDuration <= 0)
            {
                return CreateModelAsync(app);
            }

            var cacheKey = CreateCacheKey(app.Id, app.Version.ToString());

            return cache.GetOrCreateAsync(cacheKey, CacheDuration, async entry =>
            {
                return await CreateModelAsync(app);
            },
            async entry =>
            {
                var (created, hash) = await schemasHash.GetCurrentHashAsync(app.Id);

                return created < entry.Created || string.Equals(hash, entry.Hash, StringComparison.OrdinalIgnoreCase);
            });
        }

        private async Task<CacheEntry> CreateModelAsync(IAppEntity app)
        {
            var allSchemas = await serviceProvider.GetRequiredService<IAppProvider>().GetSchemasAsync(app.Id);

            var hash = await schemasHash.ComputeHashAsync(app, allSchemas);

            return new CacheEntry(
                new GraphQLModel(app,
                    allSchemas,
                    serviceProvider.GetRequiredService<SharedTypes>(),
                    serviceProvider.GetRequiredService<ISemanticLog>()),
                hash,
                SystemClock.Instance.GetCurrentInstant());
        }

        private static object CreateCacheKey(DomainId appId, string etag)
        {
            return $"GraphQLModel_{appId}_{etag}";
        }
    }
}
