// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using GraphQL;
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
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class CachingGraphQLService : IGraphQLService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IBackgroundCache cache;
        private readonly ISchemasHash schemasHash;
        private readonly IServiceProvider serviceProvider;
        private readonly GraphQLOptions options;

        private sealed record CacheEntry(GraphQLModel Model, string Hash, Instant Created);

        public IServiceProvider Services
        {
            get => serviceProvider;
        }

        public CachingGraphQLService(IBackgroundCache cache, ISchemasHash schemasHash, IServiceProvider serviceProvider, IOptions<GraphQLOptions> options)
        {
            this.cache = cache;
            this.schemasHash = schemasHash;
            this.serviceProvider = serviceProvider;
            this.options = options.Value;
        }

        public async Task<ExecutionResult> ExecuteAsync(ExecutionOptions options)
        {
            var context = ((GraphQLExecutionContext)options.UserContext).Context;

            var model = await GetModelAsync(context.App);

            return await model.ExecuteAsync(options);
        }

        public async Task<GraphQLModel> GetModelAsync(IAppEntity app)
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

            var cacheKey = CreateCacheKey(app.Id, app.Version.ToString(CultureInfo.InvariantCulture));

            return cache.GetOrCreateAsync(cacheKey, CacheDuration, async entry =>
            {
                return await CreateModelAsync(app);
            },
            async entry =>
            {
                var (created, hash) = await schemasHash.GetCurrentHashAsync(app);

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
