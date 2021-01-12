// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.LoadTests
{
    public class WritingBenchmarks : IClassFixture<WritingFixture>
    {
        public WritingFixture _ { get; }

        public WritingBenchmarks(WritingFixture fixture)
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
                5,
                10,
                20,
                50,
                100
            };

            foreach (var user in users)
            {
                foreach (var load in loads)
                {
                    yield return new object[] { user, load };
                }
            }

            yield return new object[] { 1, 50000 };
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_create_items(int numUsers, int numIterationsPerUser)
        {
            var random = new Random();

            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await _.Contents.CreateAsync(new TestEntityData { Number = random.Next() }, true);
            });
        }
    }
}
