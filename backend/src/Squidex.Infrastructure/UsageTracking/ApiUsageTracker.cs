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
        public const string CounterTotalWeight = "TotalWeight";
        public const string CounterTotalCalls = "TotalCalls";
        public const string CounterTotalElapsedMs = "TotalElapsedMs";
        private readonly IUsageTracker usageTracker;

        public ApiUsageTracker(IUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
        }

        public async Task<long> GetMonthlyWeightAsync(string key, DateTime date)
        {
            var apiKey = GetKey(key);

            var counters = await usageTracker.GetForMonthAsync(apiKey, date);

            return counters.GetInt64(CounterTotalWeight);
        }

        public Task TrackAsync(DateTime date, string key, string? category, double weight, long elapsed, long bytes)
        {
            var apiKey = GetKey(key);

            var counters = new Counters
            {
                [CounterTotalWeight] = weight,
                [CounterTotalCalls] = 1,
                [CounterTotalElapsedMs] = elapsed,
                [CounterTotalBytes] = bytes
            };

            return usageTracker.TrackAsync(date, apiKey, category, counters);
        }

        public async Task<(ApiStats Summary, Dictionary<string, List<(DateTime Date, ApiStats Stats)>> Details)> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            var apiKey = GetKey(key);

            var queries = await usageTracker.QueryAsync(apiKey, fromDate, toDate);

            var result = new Dictionary<string, List<(DateTime Date, ApiStats Stats)>>();

            var summaryBytes = 0L;
            var summaryCalls = 0L;
            var summaryElapsed = 0L;

            foreach (var (category, usages) in queries)
            {
                var resultByCategory = new List<(DateTime Date, ApiStats)>();

                foreach (var usage in usages)
                {
                    var dateBytes = usage.Counters.GetInt64(CounterTotalBytes);
                    var dateCalls = usage.Counters.GetInt64(CounterTotalCalls);
                    var dateElapsed = usage.Counters.GetInt64(CounterTotalElapsedMs);
                    var dateElapsedAvg = CalculateAverage(dateCalls, dateElapsed);

                    resultByCategory.Add((usage.Date, new ApiStats(dateCalls, dateElapsedAvg, dateBytes)));

                    summaryBytes += dateBytes;
                    summaryCalls += dateCalls;
                    summaryElapsed += dateElapsed;
                }

                result[category] = resultByCategory;
            }

            var summaryElapsedAvg = CalculateAverage(summaryCalls, summaryElapsed);

            var summary = new ApiStats(summaryCalls, summaryElapsedAvg, summaryBytes);

            return (summary, result);
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
