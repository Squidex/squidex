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
    private readonly CompletionTimer usageTimer;
    private ConcurrentDictionary<(string Key, string Category, DateOnly Date), Counters> jobs = new ConcurrentDictionary<(string Key, string Category, DateOnly Date), Counters>();
    private bool isUpdating;

    public bool HasPendingJobs => !jobs.IsEmpty || isUpdating;

    public string FallbackCategory => "*";

    public BackgroundUsageTracker(IUsageRepository usageRepository,
        ILogger<BackgroundUsageTracker> log)
    {
        this.usageRepository = usageRepository;
        this.usageTimer = new CompletionTimer(Intervall, TrackAsync, Intervall);

        this.log = log;
    }

    protected override void DisposeObject(bool disposing)
    {
        if (disposing)
        {
            usageTimer.StopAsync().Wait();
        }
    }

    public void Next()
    {
        ThrowIfDisposed();

        usageTimer.SkipCurrentDelay();
    }

    private async Task TrackAsync(
        CancellationToken ct)
    {
        try
        {
            isUpdating = true;

            var localUsages = Interlocked.Exchange(ref jobs, new ConcurrentDictionary<(string Key, string Category, DateOnly Date), Counters>());

            if (!localUsages.IsEmpty)
            {
                var updateBatch = new UsageUpdate[localUsages.Count];
                var updateIndex = 0;

                foreach (var (key, value) in localUsages)
                {
                    if (updateIndex >= updateBatch.Length)
                    {
                        break;
                    }

                    updateBatch[updateIndex].Key = key.Key;
                    updateBatch[updateIndex].Category = key.Category;
                    updateBatch[updateIndex].Counters = value;
                    updateBatch[updateIndex].Date = key.Date;

                    updateIndex++;
                }

                await usageRepository.TrackUsagesAsync(updateBatch, ct);
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to track usage in background.");
        }
        finally
        {
            isUpdating = false;
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

    public Task TrackAsync(DateOnly date, string key, string? category, Counters counters,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);
        Guard.NotNull(counters);

        ThrowIfDisposed();

        category = GetCategory(category);

        // Create a copy of the counters on add, so that we do not share it.
        jobs.AddOrUpdate((key, category, date), (_, args) => new Counters(args), (_, v, args) => v.Merge(args), counters);

        return Task.CompletedTask;
    }

    public async Task<Dictionary<string, List<(DateOnly, Counters)>>> QueryAsync(string key, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        ThrowIfDisposed();

        var result = new Dictionary<string, List<(DateOnly Date, Counters Counters)>>();

        var usageData = await usageRepository.QueryAsync(key, fromDate, toDate, ct);
        var usageGroups = usageData.GroupBy(x => GetCategory(x.Category)).ToDictionary(x => x.Key, x => x.ToList());

        if (usageGroups.Keys.Count == 0)
        {
            var enriched = new List<(DateOnly Date, Counters Counters)>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                enriched.Add((date, new Counters()));
            }

            result[FallbackCategory] = enriched;
        }

        foreach (var (category, value) in usageGroups)
        {
            var enriched = new List<(DateOnly Date, Counters Counters)>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var counters = value.Find(x => x.Date == date)?.Counters;

                enriched.Add((date, counters ?? []));
            }

            result[category] = enriched;
        }

        return result;
    }

    public Task<Counters> GetForMonthAsync(string key, DateOnly date, string? category,
        CancellationToken ct = default)
    {
        var dateFrom = new DateOnly(date.Year, date.Month, 1);
        var dateTo = dateFrom.AddMonths(1).AddDays(-1);

        return GetAsync(key, dateFrom, dateTo, category, ct);
    }

    public async Task<Counters> GetAsync(string key, DateOnly fromDate, DateOnly toDate, string? category,
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
            result.Merge(usage.Counters);
        }

        return result;
    }

    private string GetCategory(string? category)
    {
        return !string.IsNullOrWhiteSpace(category) ? category.Trim() : FallbackCategory;
    }
}
