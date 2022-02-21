// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public abstract class QueryExecutionContext : Dictionary<string, object>
    {
        private readonly SemaphoreSlim maxRequests = new SemaphoreSlim(10);
        private readonly IAssetQueryService assetQuery;
        private readonly IAssetCache assetCache;
        private readonly IContentQueryService contentQuery;
        private readonly IContentCache contentCache;

        public abstract Context Context { get; }

        public IContentCache ContentCache
        {
            get => contentCache;
        }

        public IAssetCache AssetCache
        {
            get => assetCache;
        }

        public IServiceProvider Services { get; }

        protected QueryExecutionContext(
            IAssetQueryService assetQuery,
            IAssetCache assetCache,
            IContentQueryService contentQuery,
            IContentCache contentCache,
            IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider);

            this.assetQuery = assetQuery;
            this.assetCache = assetCache;
            this.contentQuery = contentQuery;
            this.contentCache = contentCache;

            Services = serviceProvider;
        }

        public virtual Task<IEnrichedContentEntity?> FindContentAsync(string schemaIdOrName, DomainId id, long version,
            CancellationToken ct)
        {
            return contentQuery.FindAsync(Context, schemaIdOrName, id, version, ct);
        }

        public virtual async Task<IResultList<IEnrichedAssetEntity>> QueryAssetsAsync(Q q,
            CancellationToken ct)
        {
            IResultList<IEnrichedAssetEntity> assets;

            await maxRequests.WaitAsync(ct);
            try
            {
                assets = await assetQuery.QueryAsync(Context, null, q, ct);
            }
            finally
            {
                maxRequests.Release();
            }

            assetCache.SetMany(assets.Select(x => (x.Id, x))!);

            return assets;
        }

        public virtual async Task<IResultList<IEnrichedContentEntity>> QueryContentsAsync(string schemaIdOrName, Q q,
            CancellationToken ct)
        {
            IResultList<IEnrichedContentEntity> contents;

            await maxRequests.WaitAsync(ct);
            try
            {
                contents = await contentQuery.QueryAsync(Context, schemaIdOrName, q, ct);
            }
            finally
            {
                maxRequests.Release();
            }

            contentCache.SetMany(contents.Select(x => (x.Id, x))!);

            return contents;
        }

        public virtual async Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(IEnumerable<DomainId> ids,
            CancellationToken ct)
        {
            Guard.NotNull(ids);

            return await assetCache.CacheOrQueryAsync(ids, async pendingIds =>
            {
                await maxRequests.WaitAsync(ct);
                try
                {
                    var q = Q.Empty.WithIds(pendingIds).WithoutTotal();

                    return await assetQuery.QueryAsync(Context, null, q, ct);
                }
                finally
                {
                    maxRequests.Release();
                }
            });
        }

        public virtual async Task<IReadOnlyList<IEnrichedContentEntity>> GetReferencedContentsAsync(IEnumerable<DomainId> ids,
            CancellationToken ct)
        {
            Guard.NotNull(ids);

            return await contentCache.CacheOrQueryAsync(ids, async pendingIds =>
            {
                await maxRequests.WaitAsync(ct);
                try
                {
                    var q = Q.Empty.WithIds(pendingIds).WithoutTotal();

                    return await contentQuery.QueryAsync(Context, q, ct);
                }
                finally
                {
                    maxRequests.Release();
                }
            });
        }

        public T Resolve<T>() where T : class
        {
            var key = typeof(T).Name;

            if (TryGetValue(key, out var stored) && stored is T typed)
            {
                return typed;
            }

            typed = Services.GetRequiredService<T>();

            this[key] = typed;

            return typed;
        }
    }
}
