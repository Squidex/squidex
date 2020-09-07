// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class QueryExecutionContext : Dictionary<string, object>
    {
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
                asset = await assetQuery.FindAssetAsync(context, id);

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
                content = await contentQuery.FindContentAsync(context, schemaId.ToString(), id);

                if (content != null)
                {
                    cachedContents[content.Id] = content;
                }
            }

            return content;
        }

        public virtual async Task<IResultList<IEnrichedAssetEntity>> QueryAssetsAsync(string query)
        {
            var assets = await assetQuery.QueryAsync(context, null, Q.Empty.WithODataQuery(query));

            foreach (var asset in assets)
            {
                cachedAssets[asset.Id] = asset;
            }

            return assets;
        }

        public virtual async Task<IResultList<IEnrichedContentEntity>> QueryContentsAsync(string schemaIdOrName, string query)
        {
            var result = await contentQuery.QueryAsync(context, schemaIdOrName, Q.Empty.WithODataQuery(query));

            foreach (var content in result)
            {
                cachedContents[content.Id] = content;
            }

            return result;
        }

        public virtual async Task<IReadOnlyList<IEnrichedAssetEntity>> GetReferencedAssetsAsync(ICollection<DomainId> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedAssets = new HashSet<DomainId>(ids.Where(id => !cachedAssets.ContainsKey(id)));

            if (notLoadedAssets.Count > 0)
            {
                var assets = await assetQuery.QueryAsync(context, null, Q.Empty.WithIds(notLoadedAssets));

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
                var result = await contentQuery.QueryAsync(context, notLoadedContents);

                foreach (var content in result)
                {
                    cachedContents[content.Id] = content;
                }
            }

            return ids.Select(cachedContents.GetOrDefault).NotNull().ToList();
        }
    }
}
