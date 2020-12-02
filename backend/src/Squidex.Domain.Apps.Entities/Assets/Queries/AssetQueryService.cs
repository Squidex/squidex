// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
            Guard.NotNull(assetEnricher, nameof(assetEnricher));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(assetFolderRepository, nameof(assetFolderRepository));
            Guard.NotNull(queryParser, nameof(queryParser));

            this.assetEnricher = assetEnricher;
            this.assetRepository = assetRepository;
            this.assetFolderRepository = assetFolderRepository;
            this.queryParser = queryParser;
        }

        public async Task<IEnrichedAssetEntity?> FindByHashAsync(Context context, string hash, string fileName, long fileSize)
        {
            Guard.NotNull(context, nameof(context));

            var asset = await assetRepository.FindAssetAsync(context.App.Id, hash, fileName, fileSize);

            if (asset != null)
            {
                return await assetEnricher.EnrichAsync(asset, context);
            }

            return null;
        }

        public async Task<IEnrichedAssetEntity?> FindAsync(Context context, DomainId id)
        {
            Guard.NotNull(context, nameof(context));

            var asset = await assetRepository.FindAssetAsync(context.App.Id, id);

            if (asset != null)
            {
                return await assetEnricher.EnrichAsync(asset, context);
            }

            return null;
        }

        public async Task<IReadOnlyList<IAssetFolderEntity>> FindAssetFolderAsync(DomainId appId, DomainId id)
        {
            var result = new List<IAssetFolderEntity>();

            while (id != DomainId.Empty)
            {
                var folder = await assetFolderRepository.FindAssetFolderAsync(appId, id);

                if (folder == null || result.Any(x => x.Id == folder.Id))
                {
                    result.Clear();
                    break;
                }

                result.Insert(0, folder);

                id = folder.ParentId;
            }

            return result;
        }

        public async Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(Context context, DomainId parentId)
        {
            var assetFolders = await assetFolderRepository.QueryAsync(context.App.Id, parentId);

            return assetFolders;
        }

        public async Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, DomainId? parentId, Q q)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(q, nameof(q));

            q = await queryParser.ParseQueryAsync(context, q);

            var assets = await assetRepository.QueryAsync(context.App.Id, parentId, q);

            if (q.Ids != null && q.Ids.Count > 0)
            {
                assets = assets.SortSet(x => x.Id, q.Ids);
            }

            var enriched = await assetEnricher.EnrichAsync(assets, context);

            return ResultList.Create(assets.Total, enriched);
        }
    }
}
