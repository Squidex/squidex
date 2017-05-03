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
using FluentAssertions;
using Moq;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class BackgroundUsageTrackerTests
    {
        private readonly Mock<IUsageStore> usageStore = new Mock<IUsageStore>();
        private readonly BackgroundUsageTracker sut;

        public BackgroundUsageTrackerTests()
        {
            sut = new BackgroundUsageTracker(usageStore.Object);
        }

        [Fact]
        public Task Should_throw_if_tracking_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.TrackAsync("key1", 1000));
        }

        [Fact]
        public Task Should_throw_if_querying_on_disposed_object()
        {
            sut.Dispose();

            return Assert.ThrowsAsync<ObjectDisposedException>(() => sut.FindAsync("key1", DateTime.Today, DateTime.Today.AddDays(1)));
        }
        
        [Fact]
        public async Task Should_fill_missing_days()
        {
            var dateFrom = DateTime.Today;
            var dateTo = DateTime.Today.AddDays(7);

            IReadOnlyList<StoredUsage> originalDate = new List<StoredUsage>
            {
                new StoredUsage(dateFrom.AddDays(1), 10, 15),
                new StoredUsage(dateFrom.AddDays(3), 13, 18),
                new StoredUsage(dateFrom.AddDays(5), 15, 20),
                new StoredUsage(dateFrom.AddDays(7), 17, 22)
            };

            usageStore.Setup(x => x.FindAsync("key", dateFrom, dateTo)).Returns(Task.FromResult(originalDate));

            var result = await sut.FindAsync("key", dateFrom, dateTo);

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
        public async Task Should_aggregate_and_store_on_dispose()
        {
            var today = DateTime.Today;

            usageStore.Setup(x => x.TrackUsagesAsync(today, "key1", 1, 1000)).Returns(TaskHelper.Done).Verifiable();
            usageStore.Setup(x => x.TrackUsagesAsync(today, "key2", 2, 5000)).Returns(TaskHelper.Done).Verifiable();
            usageStore.Setup(x => x.TrackUsagesAsync(today, "key3", 3, 15000)).Returns(TaskHelper.Done).Verifiable();

            await sut.TrackAsync("key1", 1000);

            await sut.TrackAsync("key2", 2000);
            await sut.TrackAsync("key2", 3000);

            await sut.TrackAsync("key3", 4000);
            await sut.TrackAsync("key3", 5000);
            await sut.TrackAsync("key3", 6000);

            sut.Next();

            await Task.Delay(100);

            usageStore.VerifyAll();
        }
    }
}
