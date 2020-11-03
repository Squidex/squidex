// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
    public class QueryExecutionContext : Dictionary<string, object>
    {
        private readonly SemaphoreSlim maxRequests = new SemaphoreSlim(10);
        private readonly ConcurrentDictionary<DomainId, IEnrichedContentEntity?> cachedContents = new ConcurrentDictionary<DomainId, IEnrichedContentEntity?>();
        private readonly ConcurrentDictionary<DomainId, IEnrichedAssetEntity?> cachedAssets = new ConcurrentDictionary<DomainId, IEnrichedAssetEntity?>();
        private readonly IContentQueryService contentQuery;
        private readonly IAssetQueryService assetQuery;
        private readonly Context context;

        public Context Context
        {
            get { return context; }
        }

        public QueryExecutionContext(Context context, IAssetQueryService assetQuery, IContentQueryService contentQuery)
        {
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(context, nameof(context));

            this.assetQuery = assetQuery;
            this.contentQuery = contentQuery;
            this.context = context;
        }

        public virtual async Task<IEnrichedAssetEntity?> FindAssetAsync(DomainId id)
        {
            var asset = cachedAssets.GetOrDefault(id);

            if (asset == null)
            {
                await maxRequests.WaitAsync();
                try
                {
                    asset = await assetQuery.FindAsync(context, id);
                }
                finally
                {
                    maxRequests.Release();
                }

                if (asset != null)
                {
                    cachedAssets[asset.Id] = asset;
                }
            }

            return asset;
        }

        public virtual async Task<IEnrichedContentEntity?> FindContentAsync(DomainId schemaId, DomainId id)
        {
            var content = cachedContents.GetOrDefault(id);

            if (content == null)
            {
                await maxRequests.WaitAsync();
                try
                {
                    content = await contentQuery.FindAsync(context, schemaId.ToString(), id);
                }
                finally
                {
                    maxRequests.Release();
                }

                if (content != null)
                {
                    cachedContents[content.Id] = content;
                }
            }

            return content;
        }

        public virtual async Task<IResultList<IEnrichedAssetEntity>> QueryAssetsAsync(string odata)
        {
            var q = Q.Empty.WithODataQuery(odata);

            IResultList<IEnrichedAssetEntity> assets;

            await maxRequests.WaitAsync();
            try
            {
                assets = await assetQuery.QueryAsync(context, null, q);
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

        public virtual async Task<IResultList<IEnrichedContentEntity>> QueryContentsAsync(string schemaIdOrName, string odata)
        {
            var q = Q.Empty.WithODataQuery(odata);

            IResultList<IEnrichedContentEntity> contents;

            await maxRequests.WaitAsync();
            try
            {
                contents = await contentQuery.QueryAsync(context, schemaIdOrName, q);
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

        public virtual async Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(ICollection<DomainId> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedAssets = new HashSet<DomainId>(ids.Where(id => !cachedAssets.ContainsKey(id)));

            if (notLoadedAssets.Count > 0)
            {
                IResultList<IEnrichedAssetEntity> assets;

                await maxRequests.WaitAsync();
                try
                {
                    assets = await assetQuery.QueryAsync(context, null, Q.Empty.WithIds(notLoadedAssets));
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

        public virtual async Task<IReadOnlyList<IEnrichedContentEntity>> GetReferencedContentsAsync(ICollection<DomainId> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedContents = ids.Where(id => !cachedContents.ContainsKey(id)).ToList();

            if (notLoadedContents.Count > 0)
            {
                IResultList<IEnrichedContentEntity> contents;

                await maxRequests.WaitAsync();
                try
                {
                    contents = await contentQuery.QueryAsync(context, notLoadedContents);
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

        public async Task<IResultList<IEnrichedContentEntity>> QueryReferencingContentsAsync(string schemaIdOrName, string odata, DomainId reference)
        {
            var q = Q.Empty.WithODataQuery(odata).WithReference(reference);

            await maxRequests.WaitAsync();
            try
            {
                return await contentQuery.QueryAsync(context, schemaIdOrName, q);
            }
            finally
            {
                maxRequests.Release();
            }
        }
    }
}
