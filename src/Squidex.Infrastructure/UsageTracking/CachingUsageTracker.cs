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
        private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(10);
        private readonly IUsageTracker inner;

        public CachingUsageTracker(IUsageTracker inner, IMemoryCache cache)
            : base(cache)
        {
            Guard.NotNull(inner, nameof(inner));

            this.inner = inner;
        }

        public Task<IReadOnlyList<StoredUsage>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            return inner.QueryAsync(key, fromDate, toDate);
        }

        public Task TrackAsync(string key, double weight, double elapsedMs)
        {
            return inner.TrackAsync(key, weight, elapsedMs);
        }

        public async Task<long> GetMonthlyCallsAsync(string key, DateTime date)
        {
            Guard.NotNull(key, nameof(key));

            var cacheKey = string.Concat(key, date);

            if (Cache.TryGetValue<long>(cacheKey, out var result))
            {
                return result;
            }

            result = await inner.GetMonthlyCallsAsync(key, date);

            Cache.Set(cacheKey, result, CacheTime);

            return result;
        }
    }
}
