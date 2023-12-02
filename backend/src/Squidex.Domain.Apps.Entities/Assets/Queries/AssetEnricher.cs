// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public sealed class AssetEnricher : IAssetEnricher
{
    private readonly IEnumerable<IAssetEnricherStep> steps;

    public AssetEnricher(IEnumerable<IAssetEnricherStep> steps)
    {
        this.steps = steps;
    }

    public async Task<EnrichedAsset> EnrichAsync(Asset asset, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(asset);
        Guard.NotNull(context);

        var enriched = await EnrichAsync(Enumerable.Repeat(asset, 1), context, ct);

        return enriched[0];
    }

    public async Task<IReadOnlyList<EnrichedAsset>> EnrichAsync(IEnumerable<Asset> assets, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(assets);
        Guard.NotNull(context);

        using (var activity = Telemetry.Activities.StartActivity("AssetEnricher/EnrichAsync"))
        {
            var results = new List<EnrichedAsset>();

            if (context.App != null)
            {
                foreach (var step in steps)
                {
                    await step.EnrichAsync(context, ct);
                }
            }

            if (!assets.Any())
            {
                return results;
            }

            foreach (var asset in assets)
            {
                var result = SimpleMapper.Map(asset, new EnrichedAsset());

                results.Add(result);
            }

            if (context.App != null)
            {
                foreach (var step in steps)
                {
                    ct.ThrowIfCancellationRequested();

                    using (Telemetry.Activities.StartActivity(step.ToString()!))
                    {
                        await step.EnrichAsync(context, results, ct);
                    }
                }
            }

            activity?.SetTag("numItems", results.Count);

            return results;
        }
    }
}
