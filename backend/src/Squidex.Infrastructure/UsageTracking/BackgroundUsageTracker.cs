// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.UsageTracking;

public sealed class BackgroundUsageTracker : DisposableObjectBase, IUsageTracker
{
    private const int Intervall = 60 * 1000;
    private readonly IUsageRepository usageRepository;
    private readonly ILogger<BackgroundUsageTracker> log;
    private readonly CompletionTimer timer;
    private ConcurrentDictionary<(string Key, string Category, DateTime Date), Counters> jobs = new ConcurrentDictionary<(string Key, string Category, DateTime Date), Counters>();

    public bool ForceWrite { get; set; }

    public string FallbackCategory => "*";

    public BackgroundUsageTracker(IUsageRepository usageRepository,
        ILogger<BackgroundUsageTracker> log)
    {
        this.usageRepository = usageRepository;

        this.log = log;

        timer = new CompletionTimer(Intervall, TrackAsync, Intervall);
    }

    protected override void DisposeObject(bool disposing)
    {
        if (disposing)
        {
            timer.StopAsync().Wait();
        }
    }

    public void Next()
    {
        ThrowIfDisposed();

        timer.SkipCurrentDelay();
    }

    private async Task TrackAsync(
        CancellationToken ct)
    {
        try
        {
            var localUsages = Interlocked.Exchange(ref jobs, new ConcurrentDictionary<(string Key, string Category, DateTime Date), Counters>());

            if (!localUsages.IsEmpty)
            {
                var updates = new UsageUpdate[localUsages.Count];
                var updateIndex = 0;

                foreach (var (key, value) in localUsages)
                {
                    if (updateIndex >= updates.Length)
                    {
                        break;
                    }

                    updates[updateIndex].Key = key.Key;
                    updates[updateIndex].Category = key.Category;
                    updates[updateIndex].Counters = value;
                    updates[updateIndex].Date = key.Date;

                    updateIndex++;
                }

                if (ForceWrite)
                {
                    ct = default;
                }

                await usageRepository.TrackUsagesAsync(updates, ct);
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to track usage in background.");
        }
    }

    public Task DeleteAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNull(key);

        return usageRepository.DeleteAsync(key, ct);
    }

    public Task DeleteByKeyPatternAsync(string pattern,
        CancellationToken ct = default)
    {
        Guard.NotNull(pattern);

        return usageRepository.DeleteByKeyPatternAsync(pattern, ct);
    }

    public Task TrackAsync(DateTime date, string key, string? category, Counters counters,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);
        Guard.NotNull(counters);

        ThrowIfDisposed();

        category = GetCategory(category);

#pragma warning disable MA0105 // Use the lambda parameters instead of using a closure
        jobs.AddOrUpdate((key, category, date), counters, (k, p) => p.SumUp(counters));
#pragma warning restore MA0105 // Use the lambda parameters instead of using a closure

        return Task.CompletedTask;
    }

    public async Task<Dictionary<string, List<(DateTime, Counters)>>> QueryAsync(string key, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        ThrowIfDisposed();

        var result = new Dictionary<string, List<(DateTime Date, Counters Counters)>>();

        var usageData = await usageRepository.QueryAsync(key, fromDate, toDate, ct);
        var usageGroups = usageData.GroupBy(x => GetCategory(x.Category)).ToDictionary(x => x.Key, x => x.ToList());

        if (usageGroups.Keys.Count == 0)
        {
            var enriched = new List<(DateTime Date, Counters Counters)>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                enriched.Add((date, new Counters()));
            }

            result[FallbackCategory] = enriched;
        }

        foreach (var (category, value) in usageGroups)
        {
            var enriched = new List<(DateTime Date, Counters Counters)>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var counters = value.Find(x => x.Date == date)?.Counters;

                enriched.Add((date, counters ?? new Counters()));
            }

            result[category] = enriched;
        }

        return result;
    }

    public Task<Counters> GetForMonthAsync(string key, DateTime date, string? category,
        CancellationToken ct = default)
    {
        var dateFrom = new DateTime(date.Year, date.Month, 1);
        var dateTo = dateFrom.AddMonths(1).AddDays(-1);

        return GetAsync(key, dateFrom, dateTo, category, ct);
    }

    public async Task<Counters> GetAsync(string key, DateTime fromDate, DateTime toDate, string? category,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        ThrowIfDisposed();

        var queried = await usageRepository.QueryAsync(key, fromDate, toDate, ct);

        if (category != null)
        {
            queried = queried.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var result = new Counters();

        foreach (var usage in queried)
        {
            result.SumUp(usage.Counters);
        }

        return result;
    }

    private string GetCategory(string? category)
    {
        return !string.IsNullOrWhiteSpace(category) ? category.Trim() : FallbackCategory;
    }
}
