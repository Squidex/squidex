// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class ApiUsageTracker : IApiUsageTracker
    {
        public const string CounterTotalBytes = "TotalBytes";
        public const string CounterTotalCalls = "TotalCalls";
        public const string CounterTotalElapsedMs = "TotalElapsedMs";
        private readonly IUsageTracker usageTracker;

        public ApiUsageTracker(IUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
        }

        public async Task<long> GetMonthCostsAsync(string key, DateTime date)
        {
            var apiKey = GetKey(key);

            var counters = await usageTracker.GetForMonthAsync(apiKey, date);

            return counters.GetInt64(CounterTotalCalls);
        }

        public Task TrackAsync(DateTime date, string key, string? category, double weight, long elapsedMs, long bytes)
        {
            var apiKey = GetKey(key);

            var counters = new Counters
            {
                [CounterTotalCalls] = weight,
                [CounterTotalElapsedMs] = elapsedMs,
                [CounterTotalBytes] = bytes
            };

            return usageTracker.TrackAsync(date, apiKey, category, counters);
        }

        public async Task<(ApiStatsSummary, Dictionary<string, List<ApiStats>> Details)> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            var apiKey = GetKey(key);

            var queries = await usageTracker.QueryAsync(apiKey, fromDate, toDate);

            var details = new Dictionary<string, List<ApiStats>>();

            var summaryBytes = 0L;
            var summaryCalls = 0L;
            var summaryElapsed = 0L;

            foreach (var (category, usages) in queries)
            {
                var resultByCategory = new List<ApiStats>();

                foreach (var (date, counters) in usages)
                {
                    var dateBytes = counters.GetInt64(CounterTotalBytes);
                    var dateCalls = counters.GetInt64(CounterTotalCalls);
                    var dateElapsed = counters.GetInt64(CounterTotalElapsedMs);
                    var dateElapsedAvg = CalculateAverage(dateCalls, dateElapsed);

                    resultByCategory.Add(new ApiStats(date, dateCalls, dateElapsedAvg, dateBytes));

                    summaryBytes += dateBytes;
                    summaryCalls += dateCalls;
                    summaryElapsed += dateElapsed;
                }

                details[category] = resultByCategory;
            }

            var summaryElapsedAvg = CalculateAverage(summaryCalls, summaryElapsed);

            var summary = new ApiStatsSummary(summaryCalls, summaryElapsedAvg, summaryBytes);

            return (summary, details);
        }

        private static double CalculateAverage(long calls, long elapsed)
        {
            return calls > 0 ? Math.Round((double)elapsed / calls, 2) : 0;
        }

        private static string GetKey(string key)
        {
            Guard.NotNullOrEmpty(key);

            return $"{key}_API";
        }
    }
}
