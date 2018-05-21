// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class QueryExecutionContext
    {
        private readonly ConcurrentDictionary<Guid, IContentEntity> cachedContents = new ConcurrentDictionary<Guid, IContentEntity>();
        private readonly ConcurrentDictionary<Guid, IAssetEntity> cachedAssets = new ConcurrentDictionary<Guid, IAssetEntity>();
        private readonly IContentQueryService contentQuery;
        private readonly IAssetRepository assetRepository;
        private readonly QueryContext context;

        public QueryExecutionContext(QueryContext context,
            IAssetRepository assetRepository,
            IContentQueryService contentQuery)
        {
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(context, nameof(context));

            this.assetRepository = assetRepository;
            this.contentQuery = contentQuery;
            this.context = context;
        }

        public async Task<IAssetEntity> FindAssetAsync(Guid id)
        {
            var asset = cachedAssets.GetOrDefault(id);

            if (asset == null)
            {
                asset = await assetRepository.FindAssetAsync(id);

                if (asset != null)
                {
                    cachedAssets[asset.Id] = asset;
                }
            }

            return asset;
        }

        public async Task<IContentEntity> FindContentAsync(Guid schemaId, Guid id)
        {
            var content = cachedContents.GetOrDefault(id);

            if (content == null)
            {
                content = await contentQuery.FindContentAsync(context.WithSchemaId(schemaId), id);

                if (content != null)
                {
                    cachedContents[content.Id] = content;
                }
            }

            return content;
        }

        public async Task<IResultList<IAssetEntity>> QueryAssetsAsync(string query)
        {
            var assets = await assetRepository.QueryAsync(context.App.Id, query);

            foreach (var asset in assets)
            {
                cachedAssets[asset.Id] = asset;
            }

            return assets;
        }

        public async Task<IResultList<IContentEntity>> QueryContentsAsync(string schemaIdOrName, string query)
        {
            var result = await contentQuery.QueryAsync(context.WithSchemaName(schemaIdOrName), query);

            foreach (var content in result)
            {
                cachedContents[content.Id] = content;
            }

            return result;
        }

        public async Task<IReadOnlyList<IAssetEntity>> GetReferencedAssetsAsync(ICollection<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedAssets = new HashSet<Guid>(ids.Where(id => !cachedAssets.ContainsKey(id)));

            if (notLoadedAssets.Count > 0)
            {
                var assets = await assetRepository.QueryAsync(context.App.Id, notLoadedAssets);

                foreach (var asset in assets)
                {
                    cachedAssets[asset.Id] = asset;
                }
            }

            return ids.Select(cachedAssets.GetOrDefault).Where(x => x != null).ToList();
        }

        public async Task<IReadOnlyList<IContentEntity>> GetReferencedContentsAsync(Guid schemaId, ICollection<Guid> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            var notLoadedContents = new HashSet<Guid>(ids.Where(id => !cachedContents.ContainsKey(id)));

            if (notLoadedContents.Count > 0)
            {
                var result = await contentQuery.QueryAsync(context.WithSchemaId(schemaId), notLoadedContents);

                foreach (var content in result)
                {
                    cachedContents[content.Id] = content;
                }
            }

            return ids.Select(cachedContents.GetOrDefault).Where(x => x != null).ToList();
        }
    }
}
