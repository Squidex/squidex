// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public abstract class QueryExecutionContext : Dictionary<string, object>
    {
        private readonly SemaphoreSlim maxRequests = new SemaphoreSlim(10);
        private readonly ConcurrentDictionary<DomainId, IEnrichedContentEntity?> cachedContents = new ConcurrentDictionary<DomainId, IEnrichedContentEntity?>();
        private readonly ConcurrentDictionary<DomainId, IEnrichedAssetEntity?> cachedAssets = new ConcurrentDictionary<DomainId, IEnrichedAssetEntity?>();

        public abstract Context Context { get; }

        public IServiceProvider Services { get; }

        protected QueryExecutionContext(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider);

            Services = serviceProvider;
        }

        public virtual Task<IEnrichedContentEntity?> FindContentAsync(string schemaIdOrName, DomainId id, long version,
            CancellationToken ct)
        {
            return Resolve<IContentQueryService>().FindAsync(Context, schemaIdOrName, id, version, ct);
        }

        public virtual async Task<IResultList<IEnrichedAssetEntity>> QueryAssetsAsync(Q q,
            CancellationToken ct)
        {
            IResultList<IEnrichedAssetEntity> assets;

            await maxRequests.WaitAsync(ct);
            try
            {
                assets = await Resolve<IAssetQueryService>().QueryAsync(Context, null, q, ct);
            }
            finally
            {
                maxRequests.Release();
            }

            foreach (var asset in assets)
            {
                cachedAssets[asset.Id] = asset;
            }

            return assets;
        }

        public virtual async Task<IResultList<IEnrichedContentEntity>> QueryContentsAsync(string schemaIdOrName, Q q,
            CancellationToken ct)
        {
            IResultList<IEnrichedContentEntity> contents;

            await maxRequests.WaitAsync(ct);
            try
            {
                contents = await Resolve<IContentQueryService>().QueryAsync(Context, schemaIdOrName, q, ct);
            }
            finally
            {
                maxRequests.Release();
            }

            foreach (var content in contents)
            {
                cachedContents[content.Id] = content;
            }

            return contents;
        }

        public virtual async Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(ICollection<DomainId> ids,
            CancellationToken ct)
        {
            Guard.NotNull(ids);

            var notLoadedAssets = new HashSet<DomainId>(ids.Where(id => !cachedAssets.ContainsKey(id)));

            if (notLoadedAssets.Count > 0)
            {
                IResultList<IEnrichedAssetEntity> assets;

                await maxRequests.WaitAsync(ct);
                try
                {
                    var q = Q.Empty.WithIds(notLoadedAssets).WithoutTotal();

                    assets = await Resolve<IAssetQueryService>().QueryAsync(Context, null, q, ct);
                }
                finally
                {
                    maxRequests.Release();
                }

                foreach (var asset in assets)
                {
                    cachedAssets[asset.Id] = asset;
                }
            }

            return ids.Select(cachedAssets.GetOrDefault).NotNull().ToList();
        }

        public virtual async Task<IReadOnlyList<IEnrichedContentEntity>> GetReferencedContentsAsync(ICollection<DomainId> ids,
            CancellationToken ct)
        {
            Guard.NotNull(ids);

            var notLoadedContents = ids.Where(id => !cachedContents.ContainsKey(id)).ToList();

            if (notLoadedContents.Count > 0)
            {
                IResultList<IEnrichedContentEntity> contents;

                await maxRequests.WaitAsync(ct);
                try
                {
                    var q = Q.Empty.WithIds(notLoadedContents).WithoutTotal();

                    contents = await Resolve<IContentQueryService>().QueryAsync(Context, q, ct);
                }
                finally
                {
                    maxRequests.Release();
                }

                foreach (var content in contents)
                {
                    cachedContents[content.Id] = content;
                }
            }

            return ids.Select(cachedContents.GetOrDefault).NotNull().ToList();
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
