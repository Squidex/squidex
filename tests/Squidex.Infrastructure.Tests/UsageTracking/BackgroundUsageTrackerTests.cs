// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class BackgroundUsageTrackerTests
    {
        private readonly IUsageStore usageStore = A.Fake<IUsageStore>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly BackgroundUsageTracker sut;

        public BackgroundUsageTrackerTests()
        {
            sut = new BackgroundUsageTracker(usageStore, log);
        }

        [Fact]
        public Task Should_throw_exception_if_tracking_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.TrackAsync("MyKey1", "category1", 1, 1000));
        }

        [Fact]
        public Task Should_throw_exception_if_querying_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.QueryAsync("MyKey1", DateTime.Today, DateTime.Today.AddDays(1)));
        }

        [Fact]
        public Task Should_throw_exception_if_querying_montly_usage_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.GetMonthlyCallsAsync("MyKey1", DateTime.Today));
        }

        [Fact]
        public async Task Should_sum_up_when_getting_monthly_calls()
        {
            var date = new DateTime(2016, 1, 15);

            IReadOnlyList<StoredUsage> originalData = new List<StoredUsage>
            {
                new StoredUsage("category1", date.AddDays(1), 10, 15),
                new StoredUsage("category1", date.AddDays(3), 13, 18),
                new StoredUsage("category1", date.AddDays(5), 15, 20),
                new StoredUsage("category1", date.AddDays(7), 17, 22)
            };

            A.CallTo(() => usageStore.QueryAsync("MyKey1", new DateTime(2016, 1, 1), new DateTime(2016, 1, 31)))
                .Returns(originalData);

            var result = await sut.GetMonthlyCallsAsync("MyKey1", date);

            Assert.Equal(55, result);
        }

        [Fact]
        public async Task Should_fill_missing_days()
        {
            var f = DateTime.Today;
            var t = DateTime.Today.AddDays(4);

            var originalData = new List<StoredUsage>
            {
                new StoredUsage("MyCategory1", f.AddDays(1), 10, 15),
                new StoredUsage("MyCategory1", f.AddDays(3), 13, 18),
                new StoredUsage("MyCategory1", f.AddDays(4), 15, 20),
                new StoredUsage(null, f.AddDays(0), 17, 22),
                new StoredUsage(null, f.AddDays(2), 11, 14)
            };

            A.CallTo(() => usageStore.QueryAsync("MyKey1", f, t))
                .Returns(originalData);

            var result = await sut.QueryAsync("MyKey1", f, t);

            var expected = new Dictionary<string, List<DateUsage>>
            {
                ["MyCategory1"] = new List<DateUsage>
                {
                    new DateUsage(f.AddDays(0), 00, 00),
                    new DateUsage(f.AddDays(1), 10, 15),
                    new DateUsage(f.AddDays(2), 00, 00),
                    new DateUsage(f.AddDays(3), 13, 18),
                    new DateUsage(f.AddDays(4), 15, 20)
                },
                ["*"] = new List<DateUsage>
                {
                    new DateUsage(f.AddDays(0), 17, 22),
                    new DateUsage(f.AddDays(1), 00, 00),
                    new DateUsage(f.AddDays(2), 11, 14),
                    new DateUsage(f.AddDays(3), 00, 00),
                    new DateUsage(f.AddDays(4), 00, 00)
                }
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_fill_missing_days_with_star()
        {
            var f = DateTime.Today;
            var t = DateTime.Today.AddDays(4);

            A.CallTo(() => usageStore.QueryAsync("MyKey1", f, t))
                .Returns(new List<StoredUsage>());

            var result = await sut.QueryAsync("MyKey1", f, t);

            var expected = new Dictionary<string, List<DateUsage>>
            {
                ["*"] = new List<DateUsage>
                {
                    new DateUsage(f.AddDays(0), 00, 00),
                    new DateUsage(f.AddDays(1), 00, 00),
                    new DateUsage(f.AddDays(2), 00, 00),
                    new DateUsage(f.AddDays(3), 00, 00),
                    new DateUsage(f.AddDays(4), 00, 00)
                }
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_not_track_if_weight_less_than_zero()
        {
            await sut.TrackAsync("MyKey1", "MyCategory", -1, 1000);
            await sut.TrackAsync("MyKey1", "MyCategory", 0, 1000);

            sut.Next();
            sut.Dispose();

            A.CallTo(() => usageStore.TrackUsagesAsync(A<DateTime>.Ignored, A<string>.Ignored, A<string>.Ignored, A<double>.Ignored, A<double>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_aggregate_and_store_on_dispose()
        {
            var today = DateTime.Today;

            await sut.TrackAsync("MyKey1", "MyCategory1", 1, 1000);

            await sut.TrackAsync("MyKey2", "MyCategory1", 1.0, 2000);
            await sut.TrackAsync("MyKey2", "MyCategory1", 0.5, 3000);

            await sut.TrackAsync("MyKey3", "MyCategory1", 0.3, 4000);
            await sut.TrackAsync("MyKey3", "MyCategory1", 0.1, 5000);
            await sut.TrackAsync("MyKey3", null, 0.5, 2000);
            await sut.TrackAsync("MyKey3", null, 0.5, 6000);

            sut.Next();
            sut.Dispose();

            A.CallTo(() => usageStore.TrackUsagesAsync(today, "MyKey1", "MyCategory1", 1.0, 1000)).MustHaveHappened();
            A.CallTo(() => usageStore.TrackUsagesAsync(today, "MyKey2", "MyCategory1", 1.5, 5000)).MustHaveHappened();
            A.CallTo(() => usageStore.TrackUsagesAsync(today, "MyKey3", "MyCategory1", 0.4, 9000)).MustHaveHappened();

            A.CallTo(() => usageStore.TrackUsagesAsync(today, "MyKey3", "*", 1.0, 8000)).MustHaveHappened();
        }
    }
}
