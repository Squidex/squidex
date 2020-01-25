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
using Xunit;

namespace TestSuite.Utils
{
    public static class Run
    {
        public static async Task Parallel(int numUsers, int numIterationsPerUser, Func<Task> action, int expectedAvg = 100)
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

            Assert.Equal(0, errors);
            Assert.Equal(count, numUsers * numIterationsPerUser);

            Assert.InRange(max, 0, expectedAvg * 10);
            Assert.InRange(min, 0, expectedAvg);

            Assert.InRange(avg, 0, expectedAvg);
        }
    }
}
