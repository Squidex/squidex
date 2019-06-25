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

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetEnricher : IAssetEnricher
    {
        private readonly ITagService tagService;

        public AssetEnricher(ITagService tagService)
        {
            Guard.NotNull(tagService, nameof(tagService));

            this.tagService = tagService;
        }

        public async Task<IEnrichedAssetEntity> EnrichAsync(IAssetEntity asset)
        {
            Guard.NotNull(asset, nameof(asset));

            var enriched = await EnrichAsync(Enumerable.Repeat(asset, 1));

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedAssetEntity>> EnrichAsync(IEnumerable<IAssetEntity> assets)
        {
            Guard.NotNull(assets, nameof(assets));

            using (Profiler.TraceMethod<AssetEnricher>())
            {
                var results = new List<IEnrichedAssetEntity>();

                foreach (var group in assets.GroupBy(x => x.AppId.Id))
                {
                    var tagsById = await CalculateTags(group);

                    foreach (var asset in group)
                    {
                        var result = SimpleMapper.Map(asset, new AssetEntity());

                        result.TagNames = new HashSet<string>();

                        if (asset.Tags != null)
                        {
                            foreach (var id in asset.Tags)
                            {
                                if (tagsById.TryGetValue(id, out var name))
                                {
                                    result.TagNames.Add(name);
                                }
                            }
                        }

                        results.Add(result);
                    }
                }

                return results;
            }
        }

        private async Task<Dictionary<string, string>> CalculateTags(IGrouping<System.Guid, IAssetEntity> group)
        {
            var uniqueIds = group.Where(x => x.Tags != null).SelectMany(x => x.Tags).ToHashSet();

            return await tagService.DenormalizeTagsAsync(group.Key, TagGroups.Assets, uniqueIds);
        }
    }
}
