// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;

#pragma warning disable CS0649

namespace Squidex.Domain.Apps.Entities.Assets
{
    public partial class AssetUsageTracker : IAssetUsageTracker, IDeleter
    {
        private const string CounterTotalCount = "TotalAssets";
        private const string CounterTotalSize = "TotalSize";
        private static readonly DateTime SummaryDate;
        private readonly IUsageTracker usageTracker;

        public AssetUsageTracker(IUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            var key = GetKey(app.Id);

            return usageTracker.DeleteAsync(key, ct);
        }

        public async Task<long> GetTotalSizeAsync(DomainId appId)
        {
            var key = GetKey(appId);

            var counters = await usageTracker.GetAsync(key, SummaryDate, SummaryDate, null);

            return counters.GetInt64(CounterTotalSize);
        }

        public async Task<IReadOnlyList<AssetStats>> QueryAsync(DomainId appId, DateTime fromDate, DateTime toDate)
        {
            var enriched = new List<AssetStats>();

            var usages = await usageTracker.QueryAsync(GetKey(appId), fromDate, toDate);

            if (usages.TryGetValue("*", out var byCategory1))
            {
                AddCounters(enriched, byCategory1);
            }
            else if (usages.TryGetValue("Default", out var byCategory2))
            {
                AddCounters(enriched, byCategory2);
            }

            return enriched;
        }

        private static void AddCounters(List<AssetStats> enriched, List<(DateTime, Counters)> details)
        {
            foreach (var (date, counters) in details)
            {
                var totalCount = counters.GetInt64(CounterTotalCount);
                var totalSize = counters.GetInt64(CounterTotalSize);

                enriched.Add(new AssetStats(date, totalCount, totalSize));
            }
        }
    }
}
