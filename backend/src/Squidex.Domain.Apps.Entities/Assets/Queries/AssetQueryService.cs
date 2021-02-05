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
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class AssetQueryService : IAssetQueryService
    {
        private static readonly IResultList<IEnrichedAssetEntity> EmptyAssets = ResultList.CreateFrom<IEnrichedAssetEntity>(0);
        private readonly IAssetEnricher assetEnricher;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetLoader assetLoader;
        private readonly IAssetFolderRepository assetFolderRepository;
        private readonly AssetQueryParser queryParser;

        public AssetQueryService(
            IAssetEnricher assetEnricher,
            IAssetRepository assetRepository,
            IAssetLoader assetLoader,
            IAssetFolderRepository assetFolderRepository,
            AssetQueryParser queryParser)
        {
            Guard.NotNull(assetEnricher, nameof(assetEnricher));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(assetLoader, nameof(assetLoader));
            Guard.NotNull(assetFolderRepository, nameof(assetFolderRepository));
            Guard.NotNull(queryParser, nameof(queryParser));

            this.assetEnricher = assetEnricher;
            this.assetRepository = assetRepository;
            this.assetLoader = assetLoader;
            this.assetFolderRepository = assetFolderRepository;
            this.queryParser = queryParser;
        }

        public async Task<IReadOnlyList<IAssetFolderEntity>> FindAssetFolderAsync(DomainId appId, DomainId id)
        {
            using (Profiler.TraceMethod<AssetQueryService>())
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
        }

        public async Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(Context context, DomainId parentId)
        {
            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var assetFolders = await assetFolderRepository.QueryAsync(context.App.Id, parentId);

                return assetFolders;
            }
        }

        public async Task<IEnrichedAssetEntity?> FindByHashAsync(Context context, string hash, string fileName, long fileSize)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var asset = await assetRepository.FindAssetByHashAsync(context.App.Id, hash, fileName, fileSize);

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset);
            }
        }

        public async Task<IEnrichedAssetEntity?> FindBySlugAsync(Context context, string slug)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var asset = await assetRepository.FindAssetBySlugAsync(context.App.Id, slug);

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset);
            }
        }

        public async Task<IEnrichedAssetEntity?> FindGlobalAsync(Context context, DomainId id)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var asset = await assetRepository.FindAssetAsync(id);

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset);
            }
        }

        public async Task<IEnrichedAssetEntity?> FindAsync(Context context, DomainId id, long version = EtagVersion.Any)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                IAssetEntity? asset;

                if (version > EtagVersion.Empty)
                {
                    asset = await assetLoader.GetAsync(context.App.Id, id, version);
                }
                else
                {
                    asset = await assetRepository.FindAssetAsync(context.App.Id, id);
                }

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset);
            }
        }

        public async Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, DomainId? parentId, Q q)
        {
            Guard.NotNull(context, nameof(context));

            if (q == null)
            {
                return EmptyAssets;
            }

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                q = await queryParser.ParseAsync(context, q);

                var assets = await assetRepository.QueryAsync(context.App.Id, parentId, q);

                if (q.Ids != null && q.Ids.Count > 0)
                {
                    assets = assets.SortSet(x => x.Id, q.Ids);
                }

                return await TransformAsync(context, assets);
            }
        }

        private async Task<IResultList<IEnrichedAssetEntity>> TransformAsync(Context context, IResultList<IAssetEntity> assets)
        {
            var transformed = await TransformCoreAsync(context, assets);

            return ResultList.Create(assets.Total, transformed);
        }

        private async Task<IEnrichedAssetEntity> TransformAsync(Context context, IAssetEntity asset)
        {
            var transformed = await TransformCoreAsync(context, Enumerable.Repeat(asset, 1));

            return transformed[0];
        }

        private async Task<IReadOnlyList<IEnrichedAssetEntity>> TransformCoreAsync(Context context, IEnumerable<IAssetEntity> assets)
        {
            using (Profiler.TraceMethod<AssetQueryService>())
            {
                return await assetEnricher.EnrichAsync(assets, context);
            }
        }
    }
}
