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

        public Task<IReadOnlyDictionary<string, IReadOnlyList<DateUsage>>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            return inner.QueryAsync(key, fromDate, toDate);
        }

        public Task TrackAsync(string key, string category, double weight, double elapsedMs)
        {
            return inner.TrackAsync(key, category, weight, elapsedMs);
        }

        public Task<long> GetMonthlyCallsAsync(string key, DateTime date)
        {
            Guard.NotNull(key, nameof(key));

            var cacheKey = string.Concat(key, date);

            return Cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return inner.GetMonthlyCallsAsync(key, date);
            });
        }
    }
}
