// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking;

public interface IUsageRepository
{
    Task TrackUsagesAsync(UsageUpdate update,
        CancellationToken ct = default);

    Task TrackUsagesAsync(UsageUpdate[] updates,
        CancellationToken ct = default);

    Task<IReadOnlyList<StoredUsage>> QueryAsync(string key, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);

    Task DeleteAsync(string key,
        CancellationToken ct = default);

    Task DeleteByKeyPatternAsync(string pattern,
        CancellationToken ct = default);
}
