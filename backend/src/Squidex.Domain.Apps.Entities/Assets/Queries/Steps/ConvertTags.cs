// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Queries.Steps;

public sealed class ConvertTags : IAssetEnricherStep
{
    private readonly ITagService tagService;

    public ConvertTags(ITagService tagService)
    {
        this.tagService = tagService;
    }

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedAsset> assets,
        CancellationToken ct)
    {
        if (context.NoAssetEnrichment())
        {
            return;
        }

        var tagsById = await CalculateTagsAsync(context.App.Id, assets, ct);

        foreach (var asset in assets)
        {
            asset.TagNames = [];

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

    private async Task<Dictionary<string, string>> CalculateTagsAsync(DomainId appId, IEnumerable<EnrichedAsset> assets,
        CancellationToken ct)
    {
        var uniqueIds = assets.Where(x => x.Tags != null).SelectMany(x => x.Tags).ToHashSet();

        return await tagService.GetTagNamesAsync(appId, TagGroups.Assets, uniqueIds, ct);
    }
}
