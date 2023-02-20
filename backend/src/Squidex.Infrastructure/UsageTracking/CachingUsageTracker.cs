// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure.UsageTracking;

public sealed class CachingUsageTracker : IUsageTracker
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IUsageTracker inner;
    private readonly IMemoryCache cache;

    public string FallbackCategory => inner.FallbackCategory;

    public CachingUsageTracker(IUsageTracker inner, IMemoryCache cache)
    {
        this.inner = inner;
        this.cache = cache;
    }

    public Task DeleteAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNull(key);

        return inner.DeleteAsync(key, ct);
    }

    public Task DeleteByKeyPatternAsync(string pattern,
        CancellationToken ct = default)
    {
        Guard.NotNull(pattern);

        return inner.DeleteByKeyPatternAsync(pattern, ct);
    }

    public Task<Dictionary<string, List<(DateOnly, Counters)>>> QueryAsync(string key, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct = default)
    {
        Guard.NotNull(key);

        return inner.QueryAsync(key, fromDate, toDate, ct);
    }

    public Task TrackAsync(DateOnly date, string key, string? category, Counters counters,
        CancellationToken ct = default)
    {
        Guard.NotNull(key);

        return inner.TrackAsync(date, key, category, counters, ct);
    }

    public Task<Counters> GetForMonthAsync(string key, DateOnly date, string? category,
        CancellationToken ct = default)
    {
        Guard.NotNull(key);

        var cacheKey = $"{typeof(CachingUsageTracker)}_UsageForMonth_{key}_{date}_{category}";

        return cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return inner.GetForMonthAsync(key, date, category, ct);
        })!;
    }

    public Task<Counters> GetAsync(string key, DateOnly fromDate, DateOnly toDate, string? category,
        CancellationToken ct = default)
    {
        Guard.NotNull(key);

        var cacheKey = $"{typeof(CachingUsageTracker)}_Usage_{key}_{fromDate}_{toDate}_{category}";

        return cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            return inner.GetAsync(key, fromDate, toDate, category, ct);
        })!;
    }
}
