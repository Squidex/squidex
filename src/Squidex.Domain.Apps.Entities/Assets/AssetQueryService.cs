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
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetQueryService : IAssetQueryService
    {
        private readonly ITagService tagService;
        private readonly IAssetRepository assetRepository;

        public AssetQueryService(ITagService tagService, IAssetRepository assetRepository)
        {
            Guard.NotNull(tagService, nameof(tagService));
            Guard.NotNull(assetRepository, nameof(assetRepository));

            this.tagService = tagService;

            this.assetRepository = assetRepository;
        }

        public async Task<IAssetEntity> FindAssetAsync(QueryContext context, Guid id)
        {
            Guard.NotNull(context, nameof(context));

            var asset = await assetRepository.FindAssetAsync(id);

            if (asset != null)
            {
                await DenormalizeTagsAsync(context.App.Id, Enumerable.Repeat(asset, 1));
            }

            return asset;
        }

        public async Task<IResultList<IAssetEntity>> QueryAsync(QueryContext context, Query query)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(query, nameof(query));

            IResultList<IAssetEntity> assets;

            if (query.Ids != null)
            {
                assets = await assetRepository.QueryAsync(context.App.Id, new HashSet<Guid>(query.Ids));
                assets = Sort(assets, query.Ids);
            }
            else
            {
                assets = await assetRepository.QueryAsync(context.App.Id, query.ODataQuery);
            }

            await DenormalizeTagsAsync(context.App.Id, assets);

            return assets;
        }

        private IResultList<IAssetEntity> Sort(IResultList<IAssetEntity> assets, IList<Guid> ids)
        {
            var sorted = ids.Select(id => assets.FirstOrDefault(x => x.Id == id)).Where(x => x != null);

            return ResultList.Create(assets.Total, sorted);
        }

        private async Task DenormalizeTagsAsync(Guid appId, IEnumerable<IAssetEntity> assets)
        {
            var tags = new HashSet<string>(assets.Where(x => x.Tags != null).SelectMany(x => x.Tags).Distinct());

            var tagsById = await tagService.DenormalizeTagsAsync(appId, TagGroups.Assets, tags);

            foreach (var asset in assets)
            {
                if (asset.Tags?.Count > 0)
                {
                    var tagNames = asset.Tags.ToList();

                    asset.Tags.Clear();

                    foreach (var id in tagNames)
                    {
                        if (tagsById.TryGetValue(id, out var name))
                        {
                            asset.Tags.Add(name);
                        }
                    }
                }
                else
                {
                    asset.Tags?.Clear();
                }
            }
        }
    }
}
