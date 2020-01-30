﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IEnumerable<IAssetMetadataSource> assetMetadataSources;

        public AssetEnricher(ITagService tagService, IEnumerable<IAssetMetadataSource> assetMetadataSources)
        {
            Guard.NotNull(tagService);
            Guard.NotNull(assetMetadataSources);

            this.tagService = tagService;
            this.assetMetadataSources = assetMetadataSources;
        }

        public async Task<IEnrichedAssetEntity> EnrichAsync(IAssetEntity asset, Context context)
        {
            Guard.NotNull(asset);
            Guard.NotNull(context);

            var enriched = await EnrichAsync(Enumerable.Repeat(asset, 1), context);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedAssetEntity>> EnrichAsync(IEnumerable<IAssetEntity> assets, Context context)
        {
            Guard.NotNull(assets);
            Guard.NotNull(context);

            using (Profiler.TraceMethod<AssetEnricher>())
            {
                var results = assets.Select(x => SimpleMapper.Map(x, new AssetEntity())).ToList();

                if (ShouldEnrich(context))
                {
                    await EnrichTagsAsync(results);

                    EnrichWithMetadataText(results);
                }

                return results;
            }
        }

        private void EnrichWithMetadataText(List<AssetEntity> results)
        {
            var sb = new StringBuilder();

            void Append(string? text)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(text);
                }
            }

            foreach (var asset in results)
            {
                sb.Clear();

                foreach (var source in assetMetadataSources)
                {
                    foreach (var metadata in source.Format(asset))
                    {
                        Append(metadata);
                    }
                }

                Append(asset.FileSize.ToReadableSize());

                asset.MetadataText = sb.ToString();
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
            return context.ShouldEnrichAsset();
        }
    }
}
