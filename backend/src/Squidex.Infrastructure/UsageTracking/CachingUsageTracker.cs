﻿// ==========================================================================
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
    public sealed class CachingUsageTracker : IUsageTracker
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IUsageTracker inner;
        private readonly IMemoryCache cache;

        public CachingUsageTracker(IUsageTracker inner, IMemoryCache cache)
        {
            Guard.NotNull(inner, nameof(inner));
            Guard.NotNull(cache, nameof(cache));

            this.inner = inner;
            this.cache = cache;
        }

        public Task<Dictionary<string, List<(DateTime, Counters)>>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            Guard.NotNull(key, nameof(key));

            return inner.QueryAsync(key, fromDate, toDate);
        }

        public Task TrackAsync(DateTime date, string key, string? category, Counters counters)
        {
            Guard.NotNull(key, nameof(key));

            return inner.TrackAsync(date, key, category, counters);
        }

        public Task<Counters> GetForMonthAsync(string key, DateTime date, string? category)
        {
            Guard.NotNull(key, nameof(key));

            var cacheKey = string.Join("$", "Usage", nameof(GetForMonthAsync), key, date, category);

            return cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return inner.GetForMonthAsync(key, date, category);
            });
        }

        public Task<Counters> GetAsync(string key, DateTime fromDate, DateTime toDate, string? category)
        {
            Guard.NotNull(key, nameof(key));

            var cacheKey = string.Join("$", "Usage", nameof(GetAsync), key, fromDate, toDate, category);

            return cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return inner.GetAsync(key, fromDate, toDate, category);
            });
        }
    }
}
