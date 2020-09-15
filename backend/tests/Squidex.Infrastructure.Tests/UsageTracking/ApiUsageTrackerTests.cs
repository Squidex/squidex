﻿// ==========================================================================
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
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class ApiUsageTrackerTests
    {
        private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
        private readonly string key = Guid.NewGuid().ToString();
        private readonly string category = Guid.NewGuid().ToString();
        private readonly DateTime date = DateTime.Today;
        private readonly ApiUsageTracker sut;

        public ApiUsageTrackerTests()
        {
            sut = new ApiUsageTracker(usageTracker);
        }

        [Fact]
        public async Task Should_track_usage()
        {
            Counters? measuredCounters = null;

            A.CallTo(() => usageTracker.TrackAsync(date, $"{key}_API", null, A<Counters>.Ignored))
                .Invokes(args =>
                {
                    measuredCounters = args.GetArgument<Counters>(3)!;
                });

            await sut.TrackAsync(date, key, null, 4, 120, 1024);

            measuredCounters.Should().BeEquivalentTo(new Counters
            {
                [ApiUsageTracker.CounterTotalBytes] = 1024,
                [ApiUsageTracker.CounterTotalCalls] = 4,
                [ApiUsageTracker.CounterTotalElapsedMs] = 120
            });
        }

        [Fact]
        public async Task Should_query_calls_from_tracker()
        {
            var counters = new Counters
            {
                [ApiUsageTracker.CounterTotalCalls] = 4
            };

            A.CallTo(() => usageTracker.GetForMonthAsync($"{key}_API", date, category))
                .Returns(counters);

            var result = await sut.GetMonthCallsAsync(key, date, category);

            Assert.Equal(4, result);
        }

        [Fact]
        public async Task Should_query_bytes_from_tracker()
        {
            var counters = new Counters
            {
                [ApiUsageTracker.CounterTotalBytes] = 14
            };

            A.CallTo(() => usageTracker.GetForMonthAsync($"{key}_API", date, category))
                .Returns(counters);

            var result = await sut.GetMonthBytesAsync(key, date, category);

            Assert.Equal(14, result);
        }

        [Fact]
        public async Task Should_query_stats_from_tracker()
        {
            var dateFrom = date;
            var dateTo = dateFrom.AddDays(4);

            var counters = new Dictionary<string, List<(DateTime Date, Counters Counters)>>
            {
                ["my-category"] = new List<(DateTime Date, Counters Counters)>
                {
                    (dateFrom.AddDays(0), Counters(0, 0, 0)),
                    (dateFrom.AddDays(1), Counters(4, 100, 2048)),
                    (dateFrom.AddDays(2), Counters(0, 0, 0)),
                    (dateFrom.AddDays(3), Counters(2, 60, 1024)),
                    (dateFrom.AddDays(4), Counters(3, 30, 512))
                },
                ["*"] = new List<(DateTime Date, Counters Counters)>
                {
                    (dateFrom.AddDays(0), Counters(1, 20, 128)),
                    (dateFrom.AddDays(1), Counters(0, 0, 0)),
                    (dateFrom.AddDays(2), Counters(5, 90, 16)),
                    (dateFrom.AddDays(3), Counters(0, 0, 0)),
                    (dateFrom.AddDays(4), Counters(0, 0, 0))
                }
            };

            var forMonth = new Counters
            {
                [ApiUsageTracker.CounterTotalCalls] = 120,
                [ApiUsageTracker.CounterTotalBytes] = 400
            };

            A.CallTo(() => usageTracker.GetForMonthAsync($"{key}_API", DateTime.Today, null))
                .Returns(forMonth);

            A.CallTo(() => usageTracker.QueryAsync($"{key}_API", dateFrom, dateTo))
                .Returns(counters);

            var (summary, stats) = await sut.QueryAsync(key, dateFrom, dateTo);

            stats.Should().BeEquivalentTo(new Dictionary<string, List<ApiStats>>
            {
                ["my-category"] = new List<ApiStats>
                {
                    new ApiStats(dateFrom.AddDays(0), 0, 0, 0),
                    new ApiStats(dateFrom.AddDays(1), 4, 25, 2048),
                    new ApiStats(dateFrom.AddDays(2), 0, 0, 0),
                    new ApiStats(dateFrom.AddDays(3), 2, 30, 1024),
                    new ApiStats(dateFrom.AddDays(4), 3, 10, 512)
                },
                ["*"] = new List<ApiStats>
                {
                    new ApiStats(dateFrom.AddDays(0), 1, 20, 128),
                    new ApiStats(dateFrom.AddDays(1), 0, 0, 0),
                    new ApiStats(dateFrom.AddDays(2), 5, 18, 16),
                    new ApiStats(dateFrom.AddDays(3), 0, 0, 0),
                    new ApiStats(dateFrom.AddDays(4), 0, 0, 0)
                }
            });

            summary.Should().BeEquivalentTo(new ApiStatsSummary(20, 15, 3728, 120, 400));
        }

        private static Counters Counters(long calls, long elapsed, long bytes)
        {
            return new Counters
            {
                [ApiUsageTracker.CounterTotalBytes] = bytes,
                [ApiUsageTracker.CounterTotalCalls] = calls,
                [ApiUsageTracker.CounterTotalElapsedMs] = elapsed
            };
        }
    }
}