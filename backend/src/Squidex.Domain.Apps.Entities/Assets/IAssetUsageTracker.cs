// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IAssetUsageTracker
{
    Task<IReadOnlyList<AssetStats>> QueryByAppAsync(DomainId appId, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<IReadOnlyList<AssetStats>> QueryByTeamAsync(DomainId teamId, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<long> GetTotalSizeByAppAsync(DomainId appId,
        CancellationToken ct = default);

    Task<long> GetTotalSizeByTeamAsync(DomainId teamId,
        CancellationToken ct = default);
}
