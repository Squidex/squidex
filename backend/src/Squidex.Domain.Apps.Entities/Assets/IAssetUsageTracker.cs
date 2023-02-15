﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IAssetUsageTracker
{
    Task<IReadOnlyList<AssetStats>> QueryByAppAsync(DomainId appId, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<IReadOnlyList<AssetStats>> QueryByTeamAsync(DomainId teamId, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task<AssetCounters> GetTotalByAppAsync(DomainId appId,
        CancellationToken ct = default);

    Task<AssetCounters> GetTotalByTeamAsync(DomainId teamId,
        CancellationToken ct = default);
}

public record struct AssetStats(DateTime Date, AssetCounters Counters);

public record struct AssetCounters(long TotalCount, long TotalSize);
