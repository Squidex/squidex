// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IAssetEnricher
{
    Task<IEnrichedAssetEntity> EnrichAsync(IAssetEntity asset, Context context,
        CancellationToken ct);

    Task<IReadOnlyList<IEnrichedAssetEntity>> EnrichAsync(IEnumerable<IAssetEntity> assets, Context context,
        CancellationToken ct);
}
