// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public interface IAssetEnricherStep
{
    Task EnrichAsync(Context context, IEnumerable<EnrichedAsset> assets,
        CancellationToken ct);

    Task EnrichAsync(Context context,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
