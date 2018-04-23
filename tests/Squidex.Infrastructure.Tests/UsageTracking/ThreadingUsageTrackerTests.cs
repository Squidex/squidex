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
    public sealed class ThreadingUsageTrackerTests
    {
        private readonly MemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IUsageTracker inner = A.Fake<IUsageTracker>();
        private readonly IUsageTracker sut;

        public ThreadingUsageTrackerTests()
        {
            sut = new CachingUsageTracker(inner, cache);
        }

        [Fact]
        public async Task Should_forward_track_call()
        {
            await sut.TrackAsync("MyKey", 123, 456);

            A.CallTo(() => inner.TrackAsync("MyKey", 123, 456))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_query_call()
        {
            await sut.QueryAsync("MyKey", DateTime.MaxValue, DateTime.MinValue);

            A.CallTo(() => inner.QueryAsync("MyKey", DateTime.MaxValue, DateTime.MinValue))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_cache_monthly_usage()
        {
            A.CallTo(() => inner.GetMonthlyCallsAsync("MyKey", DateTime.Today))
                .Returns(100);

            var result1 = await sut.GetMonthlyCallsAsync("MyKey", DateTime.Today);
            var result2 = await sut.GetMonthlyCallsAsync("MyKey", DateTime.Today);

            Assert.Equal(100, result1);
            Assert.Equal(100, result2);

            A.CallTo(() => inner.GetMonthlyCallsAsync("MyKey", DateTime.Today))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
