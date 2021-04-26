// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Log;
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class BackgroundUsageTrackerTests
    {
        private readonly IUsageRepository usageStore = A.Fake<IUsageRepository>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly string key = Guid.NewGuid().ToString();
        private readonly DateTime date = DateTime.Today;
        private readonly BackgroundUsageTracker sut;

        public BackgroundUsageTrackerTests()
        {
            sut = new BackgroundUsageTracker(usageStore, log);
        }

        [Fact]
        public async Task Should_throw_exception_if_tracking_on_disposed_object()
        {
            sut.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.TrackAsync(date, key, "category1", new Counters()));
        }

        [Fact]
        public async Task Should_throw_exception_if_querying_on_disposed_object()
        {
            sut.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.QueryAsync(key, date, date.AddDays(1)));
        }

        [Fact]
        public async Task Should_throw_exception_if_querying_monthly_counters_on_disposed_object()
        {
            sut.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.GetForMonthAsync(key, date, null));
        }

        [Fact]
        public async Task Should_throw_exception_if_querying_summary_counters_on_disposed_object()
        {
            sut.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.GetAsync(key, date, date, null));
        }

        [Fact]
        public async Task Should_sum_up_if_getting_monthly_calls()
        {
            var dateFrom = new DateTime(date.Year, date.Month, 1);
            var dateTo = dateFrom.AddMonths(1).AddDays(-1);

            var originalData = new List<StoredUsage>
            {
                new StoredUsage("category1", date.AddDays(1), Counters(a: 10, b: 15)),
                new StoredUsage("category1", date.AddDays(3), Counters(a: 13, b: 18)),
                new StoredUsage("category2", date.AddDays(5), Counters(a: 15)),
                new StoredUsage("category2", date.AddDays(7), Counters(b: 22))
            };

            A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo))
                .Returns(originalData);

            var result1 = await sut.GetForMonthAsync(key, date, null);
            var result2 = await sut.GetForMonthAsync(key, date, "category2");

            Assert.Equal(38, result1["A"]);
            Assert.Equal(55, result1["B"]);

            Assert.Equal(22, result2["B"]);
        }

        [Fact]
        public async Task Should_sum_up_if_getting_last_calls_calls()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(10);

            var originalData = new List<StoredUsage>
            {
                new StoredUsage("category1", date.AddDays(1), Counters(a: 10, b: 15)),
                new StoredUsage("category1", date.AddDays(3), Counters(a: 13, b: 18)),
                new StoredUsage("category2", date.AddDays(5), Counters(a: 15)),
                new StoredUsage("category2", date.AddDays(7), Counters(b: 22))
            };

            A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo))
                .Returns(originalData);

            var result1 = await sut.GetAsync(key, dateFrom, dateTo, null);
            var result2 = await sut.GetAsync(key, dateFrom, dateTo, "category2");

            Assert.Equal(38, result1["A"]);
            Assert.Equal(55, result1["B"]);

            Assert.Equal(22, result2["B"]);
        }

        [Fact]
        public async Task Should_create_empty_results_with_default_category_is_result_is_empty()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(4);

            A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo))
                .Returns(new List<StoredUsage>());

            var result = await sut.QueryAsync(key, dateFrom, dateTo);

            var expected = new Dictionary<string, List<(DateTime Date, Counters Counters)>>
            {
                ["*"] = new List<(DateTime Date, Counters Counters)>
                {
                    (dateFrom.AddDays(0), new Counters()),
                    (dateFrom.AddDays(1), new Counters()),
                    (dateFrom.AddDays(2), new Counters()),
                    (dateFrom.AddDays(3), new Counters()),
                    (dateFrom.AddDays(4), new Counters())
                }
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_create_results_with_filled_days()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(4);

            var originalData = new List<StoredUsage>
            {
                new StoredUsage("my-category", dateFrom.AddDays(1), Counters(a: 10, b: 15)),
                new StoredUsage("my-category", dateFrom.AddDays(3), Counters(a: 13, b: 18)),
                new StoredUsage("my-category", dateFrom.AddDays(4), Counters(a: 15, b: 20)),
                new StoredUsage(null, dateFrom.AddDays(0), Counters(a: 17, b: 22)),
                new StoredUsage(null, dateFrom.AddDays(2), Counters(a: 11, b: 14))
            };

            A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo))
                .Returns(originalData);

            var result = await sut.QueryAsync(key, dateFrom, dateTo);

            var expected = new Dictionary<string, List<(DateTime Date, Counters Counters)>>
            {
                ["my-category"] = new List<(DateTime Date, Counters Counters)>
                {
                    (dateFrom.AddDays(0), Counters()),
                    (dateFrom.AddDays(1), Counters(a: 10, b: 15)),
                    (dateFrom.AddDays(2), Counters()),
                    (dateFrom.AddDays(3), Counters(a: 13, b: 18)),
                    (dateFrom.AddDays(4), Counters(a: 15, b: 20))
                },
                ["*"] = new List<(DateTime Date, Counters Counters)>
                {
                    (dateFrom.AddDays(0), Counters(a: 17, b: 22)),
                    (dateFrom.AddDays(1), Counters()),
                    (dateFrom.AddDays(2), Counters(a: 11, b: 14)),
                    (dateFrom.AddDays(3), Counters()),
                    (dateFrom.AddDays(4), Counters())
                }
            };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_aggregate_and_store_on_dispose()
        {
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            var key3 = Guid.NewGuid().ToString();

            await sut.TrackAsync(date, key1, "my-category", Counters(a: 1, b: 1000));

            await sut.TrackAsync(date, key2, "my-category", Counters(a: 1.0, b: 2000));
            await sut.TrackAsync(date, key2, "my-category", Counters(a: 0.5, b: 3000));

            await sut.TrackAsync(date, key3, "my-category", Counters(a: 0.3, b: 4000));
            await sut.TrackAsync(date, key3, "my-category", Counters(a: 0.1, b: 5000));

            await sut.TrackAsync(date, key3, null, Counters(a: 0.5, b: 2000));
            await sut.TrackAsync(date, key3, null, Counters(a: 0.5, b: 6000));

            UsageUpdate[]? updates = null;

            A.CallTo(() => usageStore.TrackUsagesAsync(A<UsageUpdate[]>._))
                .Invokes((UsageUpdate[] u) => updates = u);

            sut.Next();
            sut.Dispose();

            updates.Should().BeEquivalentTo(new[]
            {
                new UsageUpdate(date, key1, "my-category", Counters(a: 1.0, b: 1000)),
                new UsageUpdate(date, key2, "my-category", Counters(a: 1.5, b: 5000)),
                new UsageUpdate(date, key3, "my-category", Counters(a: 0.4, b: 9000)),
                new UsageUpdate(date, key3, "*", Counters(1, 8000))
            }, o => o.ComparingByMembers<UsageUpdate>());

            A.CallTo(() => usageStore.TrackUsagesAsync(A<UsageUpdate[]>._))
                .MustHaveHappened();
        }

        private static Counters Counters(double? a = null, double? b = null)
        {
            var result = new Counters();

            if (a != null)
            {
                result["A"] = a.Value;
            }

            if (b != null)
            {
                result["B"] = b.Value;
            }

            return result;
        }
    }
}
