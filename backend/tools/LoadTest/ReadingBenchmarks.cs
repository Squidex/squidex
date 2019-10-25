// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using LoadTest.Model;
using Squidex.ClientLibrary;
using Xunit;

namespace LoadTest
{
    public class ReadingBenchmarks : IClassFixture<ReadingFixture>
{
        public ReadingFixture Fixture { get; }

        public ReadingBenchmarks(ReadingFixture fixture)
        {
            Fixture = fixture;
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
        public async Task Should_return_all(int numUsers, int numIterationsPerUser)
        {
            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { OrderBy = "data/value/iv asc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_skip(int numUsers, int numIterationsPerUser)
        {
            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Skip = 5, OrderBy = "data/value/iv asc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_skip_and_top(int numUsers, int numIterationsPerUser)
        {
            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Skip = 2, Top = 5, OrderBy = "data/value/iv asc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_ordering(int numUsers, int numIterationsPerUser)
        {
            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Skip = 2, Top = 5, OrderBy = "data/value/iv desc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_filter(int numUsers, int numIterationsPerUser)
        {
            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Filter = "data/value/iv gt 3 and data/value/iv lt 7", OrderBy = "data/value/iv asc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_clients(int numUsers, int numIterationsPerUser)
        {
            await Run.Parallel(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.AppsClient.GetClientsAsync(TestClient.TestAppName);
            });
        }
    }
}
