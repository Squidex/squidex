// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class AssetQueryService : IAssetQueryService
    {
        private readonly IAssetEnricher assetEnricher;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetFolderRepository assetFolderRepository;
        private readonly AssetQueryParser queryParser;

        public AssetQueryService(
            IAssetEnricher assetEnricher,
            IAssetRepository assetRepository,
            IAssetFolderRepository assetFolderRepository,
            AssetQueryParser queryParser)
        {
            Guard.NotNull(assetEnricher);
            Guard.NotNull(assetRepository);
            Guard.NotNull(assetFolderRepository);
            Guard.NotNull(queryParser);

            this.assetEnricher = assetEnricher;
            this.assetRepository = assetRepository;
            this.assetFolderRepository = assetFolderRepository;
            this.queryParser = queryParser;
        }

        public async Task<IEnrichedAssetEntity?> FindAssetAsync(Context context, Guid id)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            if (asset != null)
            {
                return await assetEnricher.EnrichAsync(asset, context);
            }

            return null;
        }

        public async Task<IReadOnlyList<IAssetFolderEntity>> FindAssetFolderAsync(Guid id)
        {
            var result = new List<IAssetFolderEntity>();

            while (id != default)
            {
                var folder = await assetFolderRepository.FindAssetFolderAsync(id);

                if (folder == null || result.Any(x => x.Id == folder.Id))
                {
                    result.Clear();
                    break;
                }

                result.Add(folder);

                id = folder.ParentId;
            }

            return result;
        }

        public async Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(Context context, Guid parentId)
        {
            var assetFolders = await assetFolderRepository.QueryAsync(context.App.Id, parentId);

            return assetFolders;
        }

        public async Task<IReadOnlyList<IEnrichedAssetEntity>> QueryByHashAsync(Context context, Guid appId, string hash)
        {
            Guard.NotNull(hash);

            var assets = await assetRepository.QueryByHashAsync(appId, hash);

            return await assetEnricher.EnrichAsync(assets, context);
        }

        public async Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, Guid? parentId, Q query)
        {
            Guard.NotNull(context);
            Guard.NotNull(query);

            IResultList<IAssetEntity> assets;

            if (query.Ids != null && query.Ids.Count > 0)
            {
                assets = await QueryByIdsAsync(context, query);
            }
            else
            {
                assets = await QueryByQueryAsync(context, parentId, query);
            }

            var enriched = await assetEnricher.EnrichAsync(assets, context);

            return ResultList.Create(assets.Total, enriched);
        }

        private async Task<IResultList<IAssetEntity>> QueryByQueryAsync(Context context, Guid? parentId, Q query)
        {
            var parsedQuery = queryParser.ParseQuery(context, query);

            return await assetRepository.QueryAsync(context.App.Id, parentId, parsedQuery);
        }

        private async Task<IResultList<IAssetEntity>> QueryByIdsAsync(Context context, Q query)
        {
            var assets = await assetRepository.QueryAsync(context.App.Id, new HashSet<Guid>(query.Ids));

            return Sort(assets, query.Ids);
        }

        private static IResultList<IAssetEntity> Sort(IResultList<IAssetEntity> assets, IReadOnlyList<Guid> ids)
        {
            return assets.SortSet(x => x.Id, ids);
        }
    }
}
