// ==========================================================================
//  BackgroundUsageTrackerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
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

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.TrackAsync("key1", 1, 1000));
        }

        [Fact]
        public Task Should_throw_exception_if_querying_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.QueryAsync("key1", DateTime.Today, DateTime.Today.AddDays(1)));
        }

        [Fact]
        public Task Should_throw_exception_if_querying_montly_usage_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.GetMonthlyCalls("key1", DateTime.Today));
        }

        [Fact]
        public async Task Should_sum_up_when_getting_monthly_calls()
        {
            var date = new DateTime(2016, 1, 15);

            IReadOnlyList<StoredUsage> originalData = new List<StoredUsage>
            {
                new StoredUsage(date.AddDays(1), 10, 15),
                new StoredUsage(date.AddDays(3), 13, 18),
                new StoredUsage(date.AddDays(5), 15, 20),
                new StoredUsage(date.AddDays(7), 17, 22)
            };

            A.CallTo(() => usageStore.QueryAsync("key", new DateTime(2016, 1, 1), new DateTime(2016, 1, 31)))
                .Returns(originalData);

            var result = await sut.GetMonthlyCalls("key", date);

            Assert.Equal(55, result);
        }

        [Fact]
        public async Task Should_fill_missing_days()
        {
            var dateFrom = DateTime.Today;
            var dateTo = DateTime.Today.AddDays(7);

            IReadOnlyList<StoredUsage> originalData = new List<StoredUsage>
            {
                new StoredUsage(dateFrom.AddDays(1), 10, 15),
                new StoredUsage(dateFrom.AddDays(3), 13, 18),
                new StoredUsage(dateFrom.AddDays(5), 15, 20),
                new StoredUsage(dateFrom.AddDays(7), 17, 22)
            };

            A.CallTo(() => usageStore.QueryAsync("key", dateFrom, dateTo))
                .Returns(originalData);

            var result = await sut.QueryAsync("key", dateFrom, dateTo);

            result.ShouldBeEquivalentTo(new List<StoredUsage>
            {
                new StoredUsage(dateFrom.AddDays(0), 00, 00),
                new StoredUsage(dateFrom.AddDays(1), 10, 15),
                new StoredUsage(dateFrom.AddDays(2), 00, 00),
                new StoredUsage(dateFrom.AddDays(3), 13, 18),
                new StoredUsage(dateFrom.AddDays(4), 00, 00),
                new StoredUsage(dateFrom.AddDays(5), 15, 20),
                new StoredUsage(dateFrom.AddDays(6), 00, 00),
                new StoredUsage(dateFrom.AddDays(7), 17, 22)
            });
        }

        [Fact]
        public async Task Should_not_track_if_weight_less_than_zero()
        {
            await sut.TrackAsync("key1", -1, 1000);
            await sut.TrackAsync("key1", 0, 1000);

            sut.Next();
            sut.Dispose();

            A.CallTo(() => usageStore.TrackUsagesAsync(A<DateTime>.Ignored, A<string>.Ignored, A<double>.Ignored, A<long>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_aggregate_and_store_on_dispose()
        {
            var today = DateTime.Today;

            A.CallTo(() => usageStore.TrackUsagesAsync(today, "key1", 1.0, 1000))
                .Returns(TaskHelper.Done);

            A.CallTo(() => usageStore.TrackUsagesAsync(today, "key2", 1.5, 5000))
                .Returns(TaskHelper.Done);

            A.CallTo(() => usageStore.TrackUsagesAsync(today, "key3", 0.9, 15000))
                .Returns(TaskHelper.Done);

            await sut.TrackAsync("key1", 1, 1000);

            await sut.TrackAsync("key2", 1.0, 2000);
            await sut.TrackAsync("key2", 0.5, 3000);

            await sut.TrackAsync("key3", 0.3, 4000);
            await sut.TrackAsync("key3", 0.1, 5000);
            await sut.TrackAsync("key3", 0.5, 6000);

            sut.Next();
            sut.Dispose();

            A.CallTo(() => usageStore.TrackUsagesAsync(today, "key1", 1.0, 1000)).MustHaveHappened();
            A.CallTo(() => usageStore.TrackUsagesAsync(today, "key2", 1.5, 5000)).MustHaveHappened();
            A.CallTo(() => usageStore.TrackUsagesAsync(today, "key3", 0.9, 15000)).MustHaveHappened();
        }
    }
}
