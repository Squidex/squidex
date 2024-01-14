// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.LoadTests;

public class ReadingContentBenchmarks : IClassFixture<ReadingFixture>
{
    public ReadingFixture _ { get; }

    public ReadingContentBenchmarks(ReadingFixture fixture)
    {
        _ = fixture;
    }

    public static TheoryData<int, int> Loads()
    {
        int[] users =
        {
            1,
            5,
            10,
            20,
            50,
            100
        };

        int[] loads =
        {
            1,
            5,
            10,
            20,
            50,
            100,
            1000
        };

        var data = new TheoryData<int, int>();

        foreach (var user in users)
        {
            foreach (var load in loads)
            {
                data.Add(user, load);
            }
        }

        data.Add(1, 20000);

        return data;
    }

    [Theory]
    [MemberData(nameof(Loads))]
    public async Task Should_return_all(int numUsers, int numIterationsPerUser)
    {
        await Run.Parallel(numUsers, numIterationsPerUser, async () =>
        {
            await _.Contents.GetAsync(new ContentQuery { OrderBy = "data/number/iv asc" });
        });
    }

    [Theory]
    [MemberData(nameof(Loads))]
    public async Task Should_return_items_with_skip(int numUsers, int numIterationsPerUser)
    {
        await Run.Parallel(numUsers, numIterationsPerUser, async () =>
        {
            await _.Contents.GetAsync(new ContentQuery { Skip = 5, OrderBy = "data/number/iv asc" });
        });
    }

    [Theory]
    [MemberData(nameof(Loads))]
    public async Task Should_return_items_with_skip_and_top(int numUsers, int numIterationsPerUser)
    {
        await Run.Parallel(numUsers, numIterationsPerUser, async () =>
        {
            await _.Contents.GetAsync(new ContentQuery { Skip = 2, Top = 5, OrderBy = "data/number/iv asc" });
        });
    }

    [Theory]
    [MemberData(nameof(Loads))]
    public async Task Should_return_items_with_ordering(int numUsers, int numIterationsPerUser)
    {
        await Run.Parallel(numUsers, numIterationsPerUser, async () =>
        {
            await _.Contents.GetAsync(new ContentQuery { Skip = 2, Top = 5, OrderBy = "data/number/iv desc" });
        });
    }

    [Theory]
    [MemberData(nameof(Loads))]
    public async Task Should_return_items_with_filter(int numUsers, int numIterationsPerUser)
    {
        await Run.Parallel(numUsers, numIterationsPerUser, async () =>
        {
            await _.Contents.GetAsync(new ContentQuery { Filter = "data/number/iv gt 3 and data/number/iv lt 7", OrderBy = "data/number/iv asc" });
        });
    }
}
