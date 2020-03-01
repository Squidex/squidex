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
using Squidex.Infrastructure.Caching;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class CachingUsageTracker : CachingProviderBase, IUsageTracker
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IUsageTracker inner;

        public CachingUsageTracker(IUsageTracker inner, IMemoryCache cache)
            : base(cache)
        {
            Guard.NotNull(inner);

            this.inner = inner;
        }

        public Task<Dictionary<string, List<(DateTime, Counters)>>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            Guard.NotNull(key);

            return inner.QueryAsync(key, fromDate, toDate);
        }

        public Task TrackAsync(DateTime date, string key, string? category, Counters counters)
        {
            Guard.NotNull(key);

            return inner.TrackAsync(date, key, category, counters);
        }

        public Task<Counters> GetForMonthAsync(string key, DateTime date)
        {
            Guard.NotNull(key);

            var cacheKey = string.Join("$", "Usage", nameof(GetForMonthAsync), key, date);

            return Cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return inner.GetForMonthAsync(key, date);
            });
        }

        public Task<Counters> GetAsync(string key, DateTime fromDate, DateTime toDate)
        {
            Guard.NotNull(key);

            var cacheKey = string.Join("$", "Usage", nameof(GetAsync), key, fromDate, toDate);

            return Cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return inner.GetAsync(key, fromDate, toDate);
            });
        }
    }
}
