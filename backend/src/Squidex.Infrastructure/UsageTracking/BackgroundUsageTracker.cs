﻿// ==========================================================================
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
using Squidex.Infrastructure.Timers;
using Squidex.Log;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class BackgroundUsageTracker : DisposableObjectBase, IUsageTracker
    {
        private const int Intervall = 60 * 1000;
        private const string FallbackCategory = "*";
        private readonly IUsageRepository usageRepository;
        private readonly ISemanticLog log;
        private readonly CompletionTimer timer;
        private ConcurrentDictionary<(string Key, string Category, DateTime Date), Counters> jobs = new ConcurrentDictionary<(string Key, string Category, DateTime Date), Counters>();

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
                var localUsages = Interlocked.Exchange(ref jobs, new ConcurrentDictionary<(string Key, string Category, DateTime Date), Counters>());

                if (!localUsages.IsEmpty)
                {
                    var updates = new UsageUpdate[localUsages.Count];
                    var updateIndex = 0;

                    foreach (var (key, value) in localUsages)
                    {
                        updates[updateIndex].Key = key.Key;
                        updates[updateIndex].Category = key.Category;
                        updates[updateIndex].Counters = value;
                        updates[updateIndex].Date = key.Date;

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

        public Task TrackAsync(DateTime date, string key, string? category, Counters counters)
        {
            Guard.NotNullOrEmpty(key, nameof(key));
            Guard.NotNull(counters, nameof(counters));

            ThrowIfDisposed();

            category = GetCategory(category);

            jobs.AddOrUpdate((key, category, date), counters, (k, p) => p.SumUp(counters));

            return Task.CompletedTask;
        }

        public async Task<Dictionary<string, List<(DateTime, Counters)>>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            ThrowIfDisposed();

            var usages = await usageRepository.QueryAsync(key, fromDate, toDate);

            var result = new Dictionary<string, List<(DateTime Date, Counters Counters)>>();

            var categories = usages.GroupBy(x => GetCategory(x.Category)).ToDictionary(x => x.Key, x => x.ToList());

            if (categories.Keys.Count == 0)
            {
                var enriched = new List<(DateTime Date, Counters Counters)>();

                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    enriched.Add((date, new Counters()));
                }

                result[FallbackCategory] = enriched;
            }

            foreach (var (category, value) in categories)
            {
                var enriched = new List<(DateTime Date, Counters Counters)>();

                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    var counters = value.FirstOrDefault(x => x.Date == date)?.Counters;

                    enriched.Add((date, counters ?? new Counters()));
                }

                result[category] = enriched;
            }

            return result;
        }

        public Task<Counters> GetForMonthAsync(string key, DateTime date, string? category)
        {
            var dateFrom = new DateTime(date.Year, date.Month, 1);
            var dateTo = dateFrom.AddMonths(1).AddDays(-1);

            return GetAsync(key, dateFrom, dateTo, category);
        }

        public async Task<Counters> GetAsync(string key, DateTime fromDate, DateTime toDate, string? category)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            ThrowIfDisposed();

            var queried = await usageRepository.QueryAsync(key, fromDate, toDate);

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

        private static string GetCategory(string? category)
        {
            return !string.IsNullOrWhiteSpace(category) ? category.Trim() : FallbackCategory;
        }
    }
}
