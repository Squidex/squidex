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

    Task TrackAsync(DateTime date, string key, string? category, double weight, long elapsedMs, long bytes,
        CancellationToken ct = default);

    Task<long> GetMonthCallsAsync(string key, DateTime date, string? category,
        CancellationToken ct = default);

    Task<long> GetMonthBytesAsync(string key, DateTime date, string? category,
        CancellationToken ct = default);

    Task<(ApiStatsSummary, Dictionary<string, List<ApiStats>> Details)> QueryAsync(string key, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default);
}
