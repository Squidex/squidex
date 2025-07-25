﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.LoadTests;

public class ReadingBenchmarks : IClassFixture<CreatedAppFixture>
{
    private readonly ITestOutputHelper testOutput;

    public CreatedAppFixture _ { get; }

    public ReadingBenchmarks(CreatedAppFixture fixture, ITestOutputHelper testOutput)
    {
        this.testOutput = testOutput;

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
            100,
        };

        int[] loads =
        {
            1,
            5,
            10,
            20,
            50,
            100,
            1000,
        };

        var data = new TheoryData<int, int>();

        foreach (var user in users)
        {
            foreach (var load in loads)
            {
                data.Add(user, load);
            }
        }

        data.Add(1, 20_0000);

        return data;
    }

    [Theory]
    [MemberData(nameof(Loads))]
    public async Task Should_return_clients(int numUsers, int numIterationsPerUser)
    {
        await Run.Parallel(numUsers, numIterationsPerUser, async () =>
        {
            await _.Client.Apps.GetClientsAsync();
        }, 100, testOutput);
    }
}
