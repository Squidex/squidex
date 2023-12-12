// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Assets.Queries.Steps;

public sealed class EnrichWithMetadataText : IAssetEnricherStep
{
    private readonly IEnumerable<IAssetMetadataSource> assetMetadataSources;

    public EnrichWithMetadataText(IEnumerable<IAssetMetadataSource> assetMetadataSources)
    {
        this.assetMetadataSources = assetMetadataSources;
    }

    public Task EnrichAsync(Context context, IEnumerable<EnrichedAsset> assets,
        CancellationToken ct)
    {
        if (context.NoAssetEnrichment())
        {
            return Task.CompletedTask;
        }

        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            void Append(string? text)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendIfNotEmpty(", ");
                    sb.Append(text);
                }
            }

            foreach (var asset in assets)
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
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }

        return Task.CompletedTask;
    }
}
