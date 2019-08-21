// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.ClientLibrary;
using Xunit;

namespace LoadTest
{
    public class QueryBenchmarks : IClassFixture<ClientQueryFixture>
    {
        public ClientQueryFixture Fixture { get; }

        public QueryBenchmarks(ClientQueryFixture fixture)
        {
            Fixture = fixture;
        }

        public static IEnumerable<object[]> Loads()
        {
            int[] users = { 1, 5, 10, 20, 50, 100 };
            int[] loads = { 5, 10, 20, 50, 100 };

            foreach (var user in users)
            {
                foreach (var load in loads)
                {
                    yield return new object[] { user, load };
                }
            }
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_all(int numUsers, int numIterationsPerUser)
        {
            await Run(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { OrderBy = "data/value/iv asc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_skip(int numUsers, int numIterationsPerUser)
        {
            await Run(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Skip = 5, OrderBy = "data/value/iv asc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_skip_and_top(int numUsers, int numIterationsPerUser)
        {
            await Run(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Skip = 2, Top = 5, OrderBy = "data/value/iv asc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_ordering(int numUsers, int numIterationsPerUser)
        {
            await Run(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Skip = 2, Top = 5, OrderBy = "data/value/iv desc" });
            });
        }

        [Theory]
        [MemberData(nameof(Loads))]
        public async Task Should_return_items_with_filter(int numUsers, int numIterationsPerUser)
        {
            await Run(numUsers, numIterationsPerUser, async () =>
            {
                await Fixture.Client.GetAsync(new ODataQuery { Filter = "data/value/iv gt 3 and data/value/iv lt 7", OrderBy = "data/value/iv asc" });
            });
        }

        private static async Task Run(int numUsers, int numIterationsPerUser, Func<Task> action, int expectedAvg = 100)
        {
            var elapsedMs = new ConcurrentBag<long>();

            var errors = 0;

            async Task RunAsync()
            {
                for (var i = 0; i < numIterationsPerUser; i++)
                {
                    try
                    {
                        var watch = Stopwatch.StartNew();

                        await action();

                        watch.Stop();

                        elapsedMs.Add(watch.ElapsedMilliseconds);
                    }
                    catch
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
            }

            var tasks = new List<Task>();

            for (var i = 0; i < numUsers; i++)
            {
                tasks.Add(Task.Run(RunAsync));
            }

            await Task.WhenAll(tasks);

            var count = elapsedMs.Count;

            var max = elapsedMs.Max();
            var min = elapsedMs.Min();

            var avg = elapsedMs.Average();

            Assert.InRange(max, 0, expectedAvg * 10);
            Assert.InRange(min, 0, expectedAvg);

            Assert.InRange(avg, 0, expectedAvg);

            Assert.Equal(0, errors);

            Assert.Equal(count, numUsers * numIterationsPerUser);
        }
    }
}
