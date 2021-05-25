// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task<IReadOnlyList<IAssetFolderEntity>> FindAssetFolderAsync(DomainId appId, DomainId id,
            CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var result = new List<IAssetFolderEntity>();

                while (id != DomainId.Empty)
                {
                    var folder = await assetFolderRepository.FindAssetFolderAsync(appId, id, ct);

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

        public async Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(DomainId appId, DomainId parentId,
            CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var assetFolders = await assetFolderRepository.QueryAsync(appId, parentId, ct);

                return assetFolders;
            }
        }

        public async Task<IResultList<IAssetFolderEntity>> QueryAssetFoldersAsync(Context context, DomainId parentId,
            CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var assetFolders = await assetFolderRepository.QueryAsync(context.App.Id, parentId, ct);

                return assetFolders;
            }
        }

        public async Task<IEnrichedAssetEntity?> FindByHashAsync(Context context, string hash, string fileName, long fileSize,
            CancellationToken ct = default)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var asset = await assetRepository.FindAssetByHashAsync(context.App.Id, hash, fileName, fileSize, ct);

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset, ct);
            }
        }

        public async Task<IEnrichedAssetEntity?> FindBySlugAsync(Context context, string slug,
            CancellationToken ct = default)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var asset = await assetRepository.FindAssetBySlugAsync(context.App.Id, slug, ct);

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset, ct);
            }
        }

        public async Task<IEnrichedAssetEntity?> FindGlobalAsync(Context context, DomainId id,
            CancellationToken ct = default)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                var asset = await assetRepository.FindAssetAsync(id, ct);

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset, ct);
            }
        }

        public async Task<IEnrichedAssetEntity?> FindAsync(Context context, DomainId id, long version = EtagVersion.Any,
            CancellationToken ct = default)
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
                    asset = await assetRepository.FindAssetAsync(context.App.Id, id, ct);
                }

                if (asset == null)
                {
                    return null;
                }

                return await TransformAsync(context, asset, ct);
            }
        }

        public async Task<IResultList<IEnrichedAssetEntity>> QueryAsync(Context context, DomainId? parentId, Q q,
            CancellationToken ct = default)
        {
            Guard.NotNull(context, nameof(context));

            if (q == null)
            {
                return EmptyAssets;
            }

            using (Profiler.TraceMethod<AssetQueryService>())
            {
                q = await queryParser.ParseAsync(context, q);

                var assets = await assetRepository.QueryAsync(context.App.Id, parentId, q, ct);

                if (q.Ids != null && q.Ids.Count > 0)
                {
                    assets = assets.SortSet(x => x.Id, q.Ids);
                }

                return await TransformAsync(context, assets, ct);
            }
        }

        private async Task<IResultList<IEnrichedAssetEntity>> TransformAsync(Context context, IResultList<IAssetEntity> assets,
            CancellationToken ct)
        {
            var transformed = await TransformCoreAsync(context, assets, ct);

            return ResultList.Create(assets.Total, transformed);
        }

        private async Task<IEnrichedAssetEntity> TransformAsync(Context context, IAssetEntity asset,
            CancellationToken ct)
        {
            var transformed = await TransformCoreAsync(context, Enumerable.Repeat(asset, 1), ct);

            return transformed[0];
        }

        private async Task<IReadOnlyList<IEnrichedAssetEntity>> TransformCoreAsync(Context context, IEnumerable<IAssetEntity> assets,
            CancellationToken ct)
        {
            using (Profiler.TraceMethod<AssetQueryService>())
            {
                return await assetEnricher.EnrichAsync(assets, context, ct);
            }
        }
    }
}
