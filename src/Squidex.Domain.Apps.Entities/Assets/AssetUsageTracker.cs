// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.UsageTracking;

#pragma warning disable CS0649

namespace Squidex.Domain.Apps.Entities.Assets
{
    public partial class AssetUsageTracker : IAssetUsageTracker, IEventConsumer
    {
        private const string Category = "Default";
        private const string CounterTotalCount = "TotalAssets";
        private const string CounterTotalSize = "TotalSize";
        private static readonly DateTime SummaryDate;
        private readonly IUsageRepository usageStore;

        public AssetUsageTracker(IUsageRepository usageStore)
        {
            Guard.NotNull(usageStore, nameof(usageStore));

            this.usageStore = usageStore;
        }

        public async Task<long> GetTotalSizeAsync(Guid appId)
        {
            var key = GetKey(appId);

            var entries = await usageStore.QueryAsync(key, SummaryDate, SummaryDate);

            return (long)entries.Select(x => x.Counters.Get(CounterTotalSize)).FirstOrDefault();
        }

        public async Task<IReadOnlyList<AssetStats>> QueryAsync(Guid appId, DateTime fromDate, DateTime toDate)
        {
            var enriched = new List<AssetStats>();

            var usagesFlat = await usageStore.QueryAsync(GetKey(appId), fromDate, toDate);

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var stored = usagesFlat.FirstOrDefault(x => x.Date == date && x.Category == Category);

                var totalCount = 0L;
                var totalSize = 0L;

                if (stored != null)
                {
                    totalCount = (long)stored.Counters.Get(CounterTotalCount);
                    totalSize = (long)stored.Counters.Get(CounterTotalSize);
                }

                enriched.Add(new AssetStats(date, totalCount, totalSize));
            }

            return enriched;
        }
    }
}
