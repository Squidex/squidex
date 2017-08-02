// ==========================================================================
//  BackgroundUsageTracker.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
        private readonly IUsageStore usageStore;
        private readonly ISemanticLog log;
        private readonly CompletionTimer timer;
        private ConcurrentDictionary<string, Usage> usages = new ConcurrentDictionary<string, Usage>();

        public sealed class Usage
        {
            public readonly double Count;
            public readonly double ElapsedMs;

            public Usage(double elapsed, double count)
            {
                ElapsedMs = elapsed;

                Count = count;
            }

            public Usage Add(double elapsed, double weight)
            {
                return new Usage(ElapsedMs + elapsed, Count + weight);
            }
        }

        public BackgroundUsageTracker(IUsageStore usageStore, ISemanticLog log)
        {
            Guard.NotNull(usageStore, nameof(usageStore));
            Guard.NotNull(log, nameof(log));

            this.usageStore = usageStore;

            this.log = log;

            timer = new CompletionTimer(60 * 1000, ct => TrackAsync());
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

                var localUsages = Interlocked.Exchange(ref usages, new ConcurrentDictionary<string, Usage>());

                await Task.WhenAll(localUsages.Select(x =>
                    usageStore.TrackUsagesAsync(
                        today,
                        x.Key,
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

        public Task TrackAsync(string key, double weight, double elapsedMs)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            if (weight > 0)
            {
                usages.AddOrUpdate(key, _ => new Usage(elapsedMs, weight), (k, x) => x.Add(elapsedMs, weight));
            }

            return TaskHelper.Done;
        }

        public async Task<IReadOnlyList<StoredUsage>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            Guard.NotNull(key, nameof(key));

            ThrowIfDisposed();

            var originalUsages = await usageStore.QueryAsync(key, fromDate, toDate);
            var enrichedUsages = new List<StoredUsage>();

            var usagesDictionary = originalUsages.ToDictionary(x => x.Date);

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                enrichedUsages.Add(usagesDictionary.GetOrDefault(date) ?? new StoredUsage(date, 0, 0));
            }

            return enrichedUsages;
        }

        public async Task<long> GetMonthlyCalls(string key, DateTime date)
        {
            ThrowIfDisposed();

            var dateFrom = new DateTime(date.Year, date.Month, 1);
            var dateTo = dateFrom.AddMonths(1).AddDays(-1);

            var originalUsages = await usageStore.QueryAsync(key, dateFrom, dateTo);

            return originalUsages.Sum(x => x.TotalCount);
        }
    }
}
