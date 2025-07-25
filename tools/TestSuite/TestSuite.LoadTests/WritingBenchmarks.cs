﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.LoadTests;

public class WritingBenchmarks : IClassFixture<WritingFixture>
{
    public WritingFixture _ { get; }

    public WritingBenchmarks(WritingFixture fixture)
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
            100,
        };

        int[] loads =
        {
            5,
            10,
            20,
            50,
            100,
        };

        var data = new TheoryData<int, int>();

        foreach (var user in users)
        {
            foreach (var load in loads)
            {
                data.Add(user, load);
            }
        }

        data.Add(1, 50_0000);

        return data;
    }

    [Theory]
    [MemberData(nameof(Loads))]
    public async Task Should_create_items(int numUsers, int numIterationsPerUser)
    {
        var random = new Random();

        await Run.Parallel(numUsers, numIterationsPerUser, async () =>
        {
            await _.Contents.CreateAsync(new TestEntityData
            {
                Number = random.Next(),
            }, ContentCreateOptions.AsPublish);
        });
    }
}
