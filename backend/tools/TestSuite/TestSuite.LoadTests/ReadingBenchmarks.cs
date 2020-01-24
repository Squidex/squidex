﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.LoadTests
{
    public class ReadingBenchmarks : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; }

        public ReadingBenchmarks(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        public static IEnumerable<object[]> Loads()
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

            foreach (var user in users)
            {
                foreach (var load in loads)
                {
                    yield return new object[] { user, load };
                }
            }

            yield return new object[] { 1, 20000 };
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_clients(int numUsers, int numIterationsPerUser)
        {
            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await _.Apps.GetClientsAsync(_.AppName);
            });
        }
    }
}
