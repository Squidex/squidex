// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace TestSuite.LoadTests;

public static class Run
{
    public static async Task Parallel(int numUsers, int numIterationsPerUser, Func<Task> action, int expectedAvg = 100, ITestOutputHelper testOutput = null)
    {
        await action();

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

        var avg = elapsedMs.Average();

        if (testOutput != null)
        {
            testOutput.WriteLine("Total Errors: {0}/{1}", errors, numUsers * numIterationsPerUser);
            testOutput.WriteLine("Total Count: {0}/{1}", count, numUsers * numIterationsPerUser);

            testOutput.WriteLine(string.Empty);
            testOutput.WriteLine("Performance Average: {0}", avg);
            testOutput.WriteLine("Performance Max: {0}", elapsedMs.Max());
            testOutput.WriteLine("Performance Min: {0}", elapsedMs.Min());
        }

        Assert.InRange(avg, 0, expectedAvg);
    }
}
