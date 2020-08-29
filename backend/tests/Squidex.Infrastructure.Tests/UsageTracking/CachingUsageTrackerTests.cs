// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class CachingUsageTrackerTests
    {
        private readonly MemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly string key = Guid.NewGuid().ToString();
        private readonly string category = Guid.NewGuid().ToString();
        private readonly DateTime date = DateTime.Today;
        private readonly IUsageTracker inner = A.Fake<IUsageTracker>();
        private readonly IUsageTracker sut;

        public CachingUsageTrackerTests()
        {
            sut = new CachingUsageTracker(inner, cache);
        }

        [Fact]
        public async Task Should_forward_track_call()
        {
            var counters = new Counters();

            await sut.TrackAsync(date, key, "my-category", counters);

            A.CallTo(() => inner.TrackAsync(date, key, "my-category", counters))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_query_call()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(10);

            await sut.QueryAsync(key, dateFrom, dateTo);

            A.CallTo(() => inner.QueryAsync(key, dateFrom, dateTo))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_cache_monthly_usage()
        {
            var counters = new Counters();

            A.CallTo(() => inner.GetForMonthAsync(key, date, category))
                .Returns(counters);

            var result1 = await sut.GetForMonthAsync(key, date, category);
            var result2 = await sut.GetForMonthAsync(key, date, category);

            Assert.Same(counters, result1);
            Assert.Same(counters, result2);

            A.CallTo(() => inner.GetForMonthAsync(key, DateTime.Today, category))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_cache_days_usage()
        {
            var counters = new Counters();

            var dateFrom = date;
            var dateTo = dateFrom.AddDays(10);

            A.CallTo(() => inner.GetAsync(key, dateFrom, dateTo, category))
                .Returns(counters);

            var result1 = await sut.GetAsync(key, dateFrom, dateTo, category);
            var result2 = await sut.GetAsync(key, dateFrom, dateTo, category);

            Assert.Same(counters, result1);
            Assert.Same(counters, result2);

            A.CallTo(() => inner.GetAsync(key, dateFrom, dateTo, category))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_cache_queries()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(10);

            var result1 = await sut.QueryAsync(key, dateFrom, dateTo);
            var result2 = await sut.QueryAsync(key, dateFrom, dateTo);

            Assert.NotSame(result2, result1);

            A.CallTo(() => inner.QueryAsync(key, dateFrom, dateTo))
                .MustHaveHappenedTwiceOrMore();
        }
    }
}
