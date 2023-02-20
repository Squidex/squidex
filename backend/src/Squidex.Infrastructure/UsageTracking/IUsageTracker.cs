// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking;

public interface IUsageTracker
{
    string FallbackCategory { get; }

    Task TrackAsync(DateOnly date, string key, string? category, Counters counters,
        CancellationToken ct = default);

    Task<Counters> GetForMonthAsync(string key, DateOnly date, string? category,
        CancellationToken ct = default);

    Task<Counters> GetAsync(string key, DateOnly fromDate, DateOnly toDate, string? category,
        CancellationToken ct = default);

    Task<Dictionary<string, List<(DateOnly, Counters)>>> QueryAsync(string key, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct = default);

    Task DeleteAsync(string key,
        CancellationToken ct = default);

    Task DeleteByKeyPatternAsync(string pattern,
        CancellationToken ct = default);
}
