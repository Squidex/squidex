﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using System.Diagnostics;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public sealed class AssetEnricher : IAssetEnricher
{
    private readonly IEnumerable<IAssetEnricherStep> steps;

    public AssetEnricher(IEnumerable<IAssetEnricherStep> steps)
    {
        this.steps = steps;
    }

    public async Task<IEnrichedAssetEntity> EnrichAsync(IAssetEntity asset, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(asset);
        Guard.NotNull(context);

        var enriched = await EnrichAsync(Enumerable.Repeat(asset, 1), context, ct);

        return enriched[0];
    }

    public async Task<IReadOnlyList<IEnrichedAssetEntity>> EnrichAsync(IEnumerable<IAssetEntity> assets, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(assets);
        Guard.NotNull(context);

        using (var activity = Telemetry.Activities.StartActivity("AssetEnricher/EnrichAsync"))
        {
            var results = new List<AssetEntity>();

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
                var result = SimpleMapper.Map(asset, new AssetEntity());

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
