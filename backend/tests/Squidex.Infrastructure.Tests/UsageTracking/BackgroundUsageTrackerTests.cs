// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.UsageTracking;

public class BackgroundUsageTrackerTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IUsageRepository usageStore = A.Fake<IUsageRepository>();
    private readonly string key = Guid.NewGuid().ToString();
    private readonly DateTime date = DateTime.Today;
    private readonly BackgroundUsageTracker sut;

    public BackgroundUsageTrackerTests()
    {
        ct = cts.Token;

        var log = A.Fake<ILogger<BackgroundUsageTracker>>();

        sut = new BackgroundUsageTracker(usageStore, log)
        {
            ForceWrite = true
        };
    }

    [Fact]
    public async Task Should_throw_exception_if_tracking_on_disposed_object()
    {
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.TrackAsync(date, key, "category1", new Counters(), ct));
    }

    [Fact]
    public async Task Should_throw_exception_if_querying_on_disposed_object()
    {
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.QueryAsync(key, date, date.AddDays(1), ct));
    }

    [Fact]
    public async Task Should_throw_exception_if_querying_monthly_counters_on_disposed_object()
    {
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.GetForMonthAsync(key, date, null, ct));
    }

    [Fact]
    public async Task Should_throw_exception_if_querying_summary_counters_on_disposed_object()
    {
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.GetAsync(key, date, date, null, ct));
    }

    [Fact]
    public void Should_provide_fallback_category()
    {
        Assert.Equal("*", sut.FallbackCategory);
    }

    [Fact]
    public async Task Should_forward_delete_prefix_call()
    {
        await sut.DeleteByKeyPatternAsync("pattern", ct);

        A.CallTo(() => usageStore.DeleteByKeyPatternAsync("pattern", ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_forward_delete_call()
    {
        await sut.DeleteAsync(key, ct);

        A.CallTo(() => usageStore.DeleteAsync(key, ct))
            .MustHaveHappened();
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

        A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo, ct))
            .Returns(originalData);

        var actual1 = await sut.GetForMonthAsync(key, date, null, ct);
        var actual2 = await sut.GetForMonthAsync(key, date, "category2", ct);

        Assert.Equal(38, actual1["A"]);
        Assert.Equal(55, actual1["B"]);

        Assert.Equal(22, actual2["B"]);
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

        A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo, ct))
            .Returns(originalData);

        var actual1 = await sut.GetAsync(key, dateFrom, dateTo, null, ct);
        var actual2 = await sut.GetAsync(key, dateFrom, dateTo, "category2", ct);

        Assert.Equal(38, actual1["A"]);
        Assert.Equal(55, actual1["B"]);

        Assert.Equal(22, actual2["B"]);
    }

    [Fact]
    public async Task Should_create_empty_actuals_with_default_category_is_actual_is_empty()
    {
        var dateFrom = date;
        var dateTo = dateFrom.AddDays(4);

        A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo, ct))
            .Returns(new List<StoredUsage>());

        var actual = await sut.QueryAsync(key, dateFrom, dateTo, ct);

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

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_create_actuals_with_filled_days()
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

        A.CallTo(() => usageStore.QueryAsync(key, dateFrom, dateTo, ct))
            .Returns(originalData);

        var actual = await sut.QueryAsync(key, dateFrom, dateTo, ct);

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

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_write_usage_in_batches()
    {
        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();
        var key3 = Guid.NewGuid().ToString();

        await sut.TrackAsync(date, key1, "my-category", Counters(a: 1, b: 1000), ct);

        await sut.TrackAsync(date, key2, "my-category", Counters(a: 1.0, b: 2000), ct);
        await sut.TrackAsync(date, key2, "my-category", Counters(a: 0.5, b: 3000), ct);

        await sut.TrackAsync(date, key3, "my-category", Counters(a: 0.3, b: 4000), ct);
        await sut.TrackAsync(date, key3, "my-category", Counters(a: 0.1, b: 5000), ct);

        await sut.TrackAsync(date, key3, null, Counters(a: 0.5, b: 2000), ct);
        await sut.TrackAsync(date, key3, null, Counters(a: 0.5, b: 6000), ct);

        UsageUpdate[]? updates = null;

        A.CallTo(() => usageStore.TrackUsagesAsync(A<UsageUpdate[]>._, A<CancellationToken>._))
            .Invokes(args =>
            {
                updates = args.GetArgument<UsageUpdate[]>(0)!;
            });

        sut.Next();
        sut.Dispose();

        // Wait for the timer to trigger.
        await Task.Delay(500, ct);

        updates.Should().BeEquivalentTo(new[]
        {
            new UsageUpdate(date, key1, "my-category", Counters(a: 1.0, b: 1000)),
            new UsageUpdate(date, key2, "my-category", Counters(a: 1.5, b: 5000)),
            new UsageUpdate(date, key3, "my-category", Counters(a: 0.4, b: 9000)),
            new UsageUpdate(date, key3, "*", Counters(1, 8000))
        }, o => o.ComparingByMembers<UsageUpdate>());

        A.CallTo(() => usageStore.TrackUsagesAsync(A<UsageUpdate[]>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private static Counters Counters(double? a = null, double? b = null)
    {
        var actual = new Counters();

        if (a != null)
        {
            actual["A"] = a.Value;
        }

        if (b != null)
        {
            actual["B"] = b.Value;
        }

        return actual;
    }
}
