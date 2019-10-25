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
        private readonly IUsageTracker inner = A.Fake<IUsageTracker>();
        private readonly IUsageTracker sut;

        public CachingUsageTrackerTests()
        {
            sut = new CachingUsageTracker(inner, cache);
        }

        [Fact]
        public async Task Should_forward_track_call()
        {
            await sut.TrackAsync(key, "MyCategory", 123, 456);

            A.CallTo(() => inner.TrackAsync(key, "MyCategory", 123, 456))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_query_call()
        {
            await sut.QueryAsync(key, DateTime.MaxValue, DateTime.MinValue);

            A.CallTo(() => inner.QueryAsync(key, DateTime.MaxValue, DateTime.MinValue))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_cache_monthly_usage()
        {
            A.CallTo(() => inner.GetMonthlyCallsAsync(key, DateTime.Today))
                .Returns(100);

            var result1 = await sut.GetMonthlyCallsAsync(key, DateTime.Today);
            var result2 = await sut.GetMonthlyCallsAsync(key, DateTime.Today);

            Assert.Equal(100, result1);
            Assert.Equal(100, result2);

            A.CallTo(() => inner.GetMonthlyCallsAsync(key, DateTime.Today))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_cache_days_usage()
        {
            var f = DateTime.Today;
            var t = DateTime.Today.AddDays(10);

            A.CallTo(() => inner.GetPreviousCallsAsync(key, f, t))
                .Returns(120);

            var result1 = await sut.GetPreviousCallsAsync(key, f, t);
            var result2 = await sut.GetPreviousCallsAsync(key, f, t);

            Assert.Equal(120, result1);
            Assert.Equal(120, result2);

            A.CallTo(() => inner.GetPreviousCallsAsync(key, f, t))
                .MustHaveHappened(1, Times.Exactly);
        }
    }
}
