// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class CachingUsageTracker : CachingProviderBase, IUsageTracker
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private readonly IUsageTracker inner;

        public CachingUsageTracker(IUsageTracker inner, IMemoryCache cache)
            : base(cache)
        {
            Guard.NotNull(inner, nameof(inner));

            this.inner = inner;
        }

        public Task<IReadOnlyDictionary<string, IReadOnlyList<DateUsage>>> QueryAsync(Guid appId, DateTime fromDate, DateTime toDate)
        {
            return inner.QueryAsync(appId, fromDate, toDate);
        }

        public Task TrackAsync(Guid appId, string category, double weight, double elapsedMs)
        {
            return inner.TrackAsync(appId, category, weight, elapsedMs);
        }

        public Task<long> GetMonthlyCallsAsync(Guid appId, DateTime date)
        {
            var cacheKey = string.Concat(appId, date);

            return Cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return inner.GetMonthlyCallsAsync(appId, date);
            });
        }
    }
}
