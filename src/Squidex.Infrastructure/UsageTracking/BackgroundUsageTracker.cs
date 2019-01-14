// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class BackgroundUsageTracker : DisposableObjectBase, IUsageTracker
    {
        public const string CounterTotalCalls = "TotalCalls";
        public const string CounterTotalElapsedMs = "TotalElapsedMs";

        private const string FallbackCategory = "*";
        private const int Intervall = 60 * 1000;
        private readonly IUsageRepository usageRepository;
        private readonly ISemanticLog log;
        private readonly CompletionTimer timer;
        private ConcurrentDictionary<(string Key, string Category), Usage> usages = new ConcurrentDictionary<(string Key, string Category), Usage>();

        public BackgroundUsageTracker(IUsageRepository usageRepository, ISemanticLog log)
        {
            Guard.NotNull(usageRepository, nameof(usageRepository));
            Guard.NotNull(log, nameof(log));

            this.usageRepository = usageRepository;

            this.log = log;

            timer = new CompletionTimer(Intervall, ct => TrackAsync(), Intervall);
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

        private async Task TrackAsync()
        {
            try
            {
                var today = DateTime.Today;

                var localUsages = Interlocked.Exchange(ref usages, new ConcurrentDictionary<(string Key, string Category), Usage>());

                if (localUsages.Count > 0)
                {
                    var updates = new UsageUpdate[localUsages.Count];
                    var updateIndex = 0;

                    foreach (var kvp in localUsages)
                    {
                        var counters = new Counters
                        {
                            [CounterTotalCalls] = kvp.Value.Count,
                            [CounterTotalElapsedMs] = kvp.Value.ElapsedMs
                        };

                        updates[updateIndex].Key = kvp.Key.Key;
                        updates[updateIndex].Category = kvp.Key.Category;
                        updates[updateIndex].Counters = counters;
                        updates[updateIndex].Date = today;

                        updateIndex++;
                    }

                    await usageRepository.TrackUsagesAsync(updates);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "TrackUsage")
                    .WriteProperty("status", "Failed"));
            }
        }

        public Task TrackAsync(string key, string category, double weight, double elapsedMs)
        {
            key = GetKey(key);

            ThrowIfDisposed();

            if (weight > 0)
            {
                category = GetCategory(category);

                usages.AddOrUpdate((key, category), _ => new Usage(elapsedMs, weight), (k, x) => x.Add(elapsedMs, weight));
            }

            return TaskHelper.Done;
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<DateUsage>>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            key = GetKey(key);

            ThrowIfDisposed();

            var usagesFlat = await usageRepository.QueryAsync(key, fromDate, toDate);
            var usagesByCategory = usagesFlat.GroupBy(x => GetCategory(x.Category)).ToDictionary(x => x.Key, x => x.ToList());

            var result = new Dictionary<string, IReadOnlyList<DateUsage>>();

            IEnumerable<string> categories = usagesByCategory.Keys;

            if (usagesByCategory.Count == 0)
            {
                var enriched = new List<DateUsage>();

                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    enriched.Add(new DateUsage(date, 0, 0));
                }

                result[FallbackCategory] = enriched;
            }
            else
            {
                foreach (var category in categories)
                {
                    var enriched = new List<DateUsage>();

                    var usagesDictionary = usagesByCategory[category].ToDictionary(x => x.Date);

                    for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                    {
                        var stored = usagesDictionary.GetOrDefault(date);

                        var totalCount = 0L;
                        var totalElapsedMs = 0L;

                        if (stored != null)
                        {
                            totalCount = (long)stored.Counters.Get(CounterTotalCalls);
                            totalElapsedMs = (long)stored.Counters.Get(CounterTotalElapsedMs);
                        }

                        enriched.Add(new DateUsage(date, totalCount, totalElapsedMs));
                    }

                    result[category] = enriched;
                }
            }

            return result;
        }

        public Task<long> GetMonthlyCallsAsync(string key, DateTime date)
        {
            return GetPreviousCallsAsync(key, new DateTime(date.Year, date.Month, 1), date);
        }

        public async Task<long> GetPreviousCallsAsync(string key, DateTime fromDate, DateTime toDate)
        {
            key = GetKey(key);

            ThrowIfDisposed();

            var originalUsages = await usageRepository.QueryAsync(key, fromDate, toDate);

            return originalUsages.Sum(x => (long)x.Counters.Get(CounterTotalCalls));
        }

        private static string GetCategory(string category)
        {
            return !string.IsNullOrWhiteSpace(category) ? category.Trim() : "*";
        }

        private static string GetKey(string key)
        {
            Guard.NotNull(key, nameof(key));

            return $"{key}_API";
        }
    }
}
