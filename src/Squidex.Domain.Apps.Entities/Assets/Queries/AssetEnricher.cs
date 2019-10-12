// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public sealed class AssetEnricher : IAssetEnricher
    {
        private readonly ITagService tagService;

        public AssetEnricher(ITagService tagService)
        {
            Guard.NotNull(tagService, nameof(tagService));

            this.tagService = tagService;
        }

        public async Task<IEnrichedAssetEntity> EnrichAsync(IAssetEntity asset, Context context)
        {
            Guard.NotNull(asset, nameof(asset));
            Guard.NotNull(context, nameof(context));

            var enriched = await EnrichAsync(Enumerable.Repeat(asset, 1), context);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedAssetEntity>> EnrichAsync(IEnumerable<IAssetEntity> assets, Context context)
        {
            Guard.NotNull(assets, nameof(assets));
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetEnricher>())
            {
                var results = assets.Select(x => SimpleMapper.Map(x, new AssetEntity())).ToList();

                if (ShouldEnrich(context))
                {
                    await EnrichTagsAsync(results);
                }

                return results;
            }
        }

        private async Task EnrichTagsAsync(List<AssetEntity> assets)
        {
            foreach (var group in assets.GroupBy(x => x.AppId.Id))
            {
                var tagsById = await CalculateTags(group);

                foreach (var asset in group)
                {
                    asset.TagNames = new HashSet<string>();

                    if (asset.Tags != null)
                    {
                        foreach (var id in asset.Tags)
                        {
                            if (tagsById.TryGetValue(id, out var name))
                            {
                                asset.TagNames.Add(name);
                            }
                        }
                    }
                }
            }
        }

        private async Task<Dictionary<string, string>> CalculateTags(IGrouping<System.Guid, IAssetEntity> group)
        {
            var uniqueIds = group.Where(x => x.Tags != null).SelectMany(x => x.Tags).ToHashSet();

            return await tagService.DenormalizeTagsAsync(group.Key, TagGroups.Assets, uniqueIds);
        }

        private static bool ShouldEnrich(Context context)
        {
            return !context.IsNoAssetEnrichment();
        }
    }
}
