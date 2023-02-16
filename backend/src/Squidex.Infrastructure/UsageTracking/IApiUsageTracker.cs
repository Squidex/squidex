// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking;

public interface IApiUsageTracker
{
    Task DeleteAsync(string key,
        CancellationToken ct = default);

    Task TrackAsync(DateOnly date, string key, string? category, double weight, long elapsedMs, long bytes,
        CancellationToken ct = default);

    Task<long> GetMonthCallsAsync(string key, DateOnly date, string? category,
        CancellationToken ct = default);

    Task<long> GetMonthBytesAsync(string key, DateOnly date, string? category,
        CancellationToken ct = default);

    Task<(ApiStatsSummary, Dictionary<string, List<ApiStats>> Details)> QueryAsync(string key, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct = default);
}
