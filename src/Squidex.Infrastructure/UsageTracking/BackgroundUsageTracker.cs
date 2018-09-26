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
        private const string FallbackCategory = "*";
        private const int Intervall = 60 * 1000;
        private readonly IUsageStore usageStore;
        private readonly ISemanticLog log;
        private readonly CompletionTimer timer;
        private ConcurrentDictionary<(string Key, string Category), Usage> usages = new ConcurrentDictionary<(string Key, string Category), Usage>();

        public BackgroundUsageTracker(IUsageStore usageStore, ISemanticLog log)
        {
            Guard.NotNull(usageStore, nameof(usageStore));
            Guard.NotNull(log, nameof(log));

            this.usageStore = usageStore;

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

                await Task.WhenAll(localUsages.Select(x =>
                    usageStore.TrackUsagesAsync(
                        today,
                        x.Key.Key,
                        x.Key.Category,
                        x.Value.Count,
                        x.Value.ElapsedMs)));
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
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            if (weight > 0)
            {
                category = CleanCategory(category);

                usages.AddOrUpdate((key, category), _ => new Usage(elapsedMs, weight), (k, x) => x.Add(elapsedMs, weight));
            }

            return TaskHelper.Done;
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<DateUsage>>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            var usagesFlat = await usageStore.QueryAsync(key, fromDate, toDate);
            var usagesByCategory = usagesFlat.GroupBy(x => CleanCategory(x.Category)).ToDictionary(x => x.Key, x => x.ToList());

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

                        enriched.Add(new DateUsage(date, stored?.TotalCount ?? 0, stored?.TotalElapsedMs ?? 0));
                    }

                    result[category] = enriched;
                }
            }

            return result;
        }

        public async Task<long> GetMonthlyCallsAsync(string key, DateTime date)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            var dateFrom = new DateTime(date.Year, date.Month, 1);
            var dateTo = dateFrom.AddMonths(1).AddDays(-1);

            var originalUsages = await usageStore.QueryAsync(key, dateFrom, dateTo);

            return originalUsages.Sum(x => x.TotalCount);
        }

        private static string CleanCategory(string category)
        {
            return !string.IsNullOrWhiteSpace(category) ? category.Trim() : "*";
        }
    }
}
