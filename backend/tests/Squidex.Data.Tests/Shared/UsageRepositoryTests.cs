// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Shared;

public abstract class UsageRepositoryTests
{
    private readonly string knownKey = "3e764e15-3cf5-427f-bb6f-f0fa29a40a2d";
    private readonly DateOnly startDate = new DateOnly(2023, 12, 11);

    protected abstract Task<IUsageRepository> CreateSutAsync();

    private async Task<IUsageRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if ((await sut.QueryAsync(knownKey, DateOnly.MinValue, DateOnly.MaxValue)).Count > 0)
        {
            return sut;
        }

        var otherKey = Guid.NewGuid().ToString();

        var writes = new UsageUpdate[]
        {
            new UsageUpdate(startDate, knownKey, "Category1",
                new Counters
                {
                    ["Key1"] = 1,
                    ["key2"] = 2,
                }),
            new UsageUpdate(startDate, knownKey, "Category2",
                new Counters
                {
                    ["Key1"] = 3,
                    ["key2"] = 4,
                }),
            new UsageUpdate(startDate.AddDays(1), knownKey, "Category1",
                new Counters
                {
                    ["Key1"] = 5,
                    ["key2"] = 6,
                }),
            new UsageUpdate(startDate.AddDays(2), knownKey, "Category2",
                new Counters
                {
                    ["Key1"] = 7,
                    ["key2"] = 8,
                }),
            new UsageUpdate(startDate, otherKey, "Category2",
                new Counters
                {
                    ["Key1"] = 9,
                    ["key2"] = 10,
                }),
        };

        await sut.TrackUsagesAsync(writes);
        return sut;
    }

    public async Task Should_query_results()
    {
        var sut = await CreateAndPrepareSutAsync();

        var result = await sut.QueryAsync(knownKey, startDate, startDate.AddDays(1));

        result.Should().BeEquivalentTo(new StoredUsage[]
        {
            new StoredUsage("Category1", startDate,
                new Counters
                {
                    ["Key1"] = 1,
                    ["key2"] = 2,
                }),
            new StoredUsage("Category2", startDate,
                new Counters
                {
                    ["Key1"] = 3,
                    ["key2"] = 4,
                }),
            new StoredUsage("Category3", startDate.AddDays(1),
                new Counters
                {
                    ["Key1"] = 5,
                    ["key2"] = 6,
                }),
        });
    }

    [Fact]
    public async Task Should_delete_by_key()
    {
        var sut = await CreateSutAsync();

        var key1 = Guid.NewGuid().ToString();
        var key2 = Guid.NewGuid().ToString();

        var writes = new UsageUpdate[]
        {
            new UsageUpdate(startDate, key1, "Category1",
                new Counters
                {
                    ["Key1"] = 1,
                    ["key2"] = 2,
                }),
            new UsageUpdate(startDate, key1, "Category1",
                new Counters
                {
                    ["Key1"] = 3,
                    ["key2"] = 4,
                }),
        };

        await sut.TrackUsagesAsync(writes);

        await sut.DeleteAsync(key1);

        var byKey1 = await sut.QueryAsync(key1, DateOnly.MinValue, DateOnly.MaxValue);
        Assert.Empty(byKey1);

        await sut.DeleteAsync(key2);

        var byKey2 = await sut.QueryAsync(key2, DateOnly.MinValue, DateOnly.MaxValue);
        Assert.Empty(byKey2);
    }

    [Fact]
    public async Task Should_update_in_parallel()
    {
        var sut = await CreateSutAsync();

        var sharedKey = Guid.NewGuid().ToString();

        await Parallel.ForEachAsync(Enumerable.Range(0, 20), async (_, ct) =>
        {
            var writes = new UsageUpdate[]
            {
                new UsageUpdate(startDate, sharedKey, "Category",
                    new Counters
                    {
                        ["Key"] = 1,
                    }),
            };

            await sut.TrackUsagesAsync(writes, ct);
        });

        var result = await sut.QueryAsync(sharedKey, DateOnly.MinValue, DateOnly.MaxValue);

        result.Should().BeEquivalentTo(new StoredUsage[]
        {
            new StoredUsage("Category", startDate,
                new Counters
                {
                    ["Key"] = 20,
                }),
        });
    }
}
