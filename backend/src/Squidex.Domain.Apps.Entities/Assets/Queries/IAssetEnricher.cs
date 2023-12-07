// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public interface IAssetEnricher
{
    Task<EnrichedAsset> EnrichAsync(Asset asset, Context context,
        CancellationToken ct);

    Task<IReadOnlyList<EnrichedAsset>> EnrichAsync(IEnumerable<Asset> assets, Context context,
        CancellationToken ct);
}
