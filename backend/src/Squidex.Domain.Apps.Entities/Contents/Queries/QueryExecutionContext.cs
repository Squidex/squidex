// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public abstract class QueryExecutionContext : Dictionary<string, object>
    {
        private readonly SemaphoreSlim maxRequests = new SemaphoreSlim(10);
        private readonly ConcurrentDictionary<DomainId, IEnrichedContentEntity?> cachedContents = new ConcurrentDictionary<DomainId, IEnrichedContentEntity?>();
        private readonly ConcurrentDictionary<DomainId, IEnrichedAssetEntity?> cachedAssets = new ConcurrentDictionary<DomainId, IEnrichedAssetEntity?>();
        private readonly IContentQueryService contentQuery;
        private readonly IAssetQueryService assetQuery;

        public abstract Context Context { get; }

        protected QueryExecutionContext(IAssetQueryService assetQuery, IContentQueryService contentQuery)
        {
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(contentQuery, nameof(contentQuery));

            this.assetQuery = assetQuery;
            this.contentQuery = contentQuery;
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
                contents = await contentQuery.QueryAsync(Context, schemaIdOrName, q, ct);
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
            Guard.NotNull(ids, nameof(ids));

            var notLoadedAssets = new HashSet<DomainId>(ids.Where(id => !cachedAssets.ContainsKey(id)));

            if (notLoadedAssets.Count > 0)
            {
                IResultList<IEnrichedAssetEntity> assets;

                await maxRequests.WaitAsync(ct);
                try
                {
                    assets = await assetQuery.QueryAsync(Context, null, Q.Empty.WithIds(notLoadedAssets).WithoutTotal(), ct);
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
            Guard.NotNull(ids, nameof(ids));

            var notLoadedContents = ids.Where(id => !cachedContents.ContainsKey(id)).ToList();

            if (notLoadedContents.Count > 0)
            {
                IResultList<IEnrichedContentEntity> contents;

                await maxRequests.WaitAsync(ct);
                try
                {
                    contents = await contentQuery.QueryAsync(Context, Q.Empty.WithIds(notLoadedContents).WithoutTotal(), ct);
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
    }
}
