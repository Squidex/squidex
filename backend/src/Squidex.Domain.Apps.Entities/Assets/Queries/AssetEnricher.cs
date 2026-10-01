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

public sealed class AssetEnricher(IEnumerable<IAssetEnricherStep> steps) : IAssetEnricher
{
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
            var results = assets.Select(x => SimpleMapper.Map(x, new EnrichedAsset())).ToList();

            await RunStepsAsync(steps, results, context, ct);

            activity?.SetTag("numItems", results.Count);

            return results;
        }
    }

    public async Task EnrichCachedAsync(IReadOnlyList<EnrichedAsset> assets, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(assets);
        Guard.NotNull(context);

        using (Telemetry.Activities.StartActivity("AssetEnricher/EnrichCachedAsync"))
        {
            await RunStepsAsync(steps.Where(x => x.RunOnCachedResults), assets, context, ct);
        }
    }

    private static async Task RunStepsAsync(IEnumerable<IAssetEnricherStep> stepsToRun, IReadOnlyList<EnrichedAsset> assets, Context context,
        CancellationToken ct)
    {
        if (context.App == null)
        {
            return;
        }

        foreach (var step in stepsToRun)
        {
            await step.EnrichAsync(context, ct);
        }

        if (assets.Count == 0)
        {
            return;
        }

        foreach (var step in stepsToRun)
        {
            ct.ThrowIfCancellationRequested();

            using (Telemetry.Activities.StartActivity(step.ToString()!))
            {
                await step.EnrichAsync(context, assets, ct);
            }
        }
    }
}
