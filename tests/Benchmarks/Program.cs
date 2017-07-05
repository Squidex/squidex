// ==========================================================================
//  Program.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Benchmarks.Tests;

namespace Benchmarks
{
    public static class Program
    {
        private static readonly List<IBenchmark> Benchmarks = new List<IBenchmark>
        {
            new AppendToEventStore(),
            new AppendToEventStoreParallel()
        };

        public static void Main(string[] args)
        {
            var id = args.Length > 0 ? args[0] : string.Empty;

            var benchmark = Benchmarks.Find(x => x.Id == id);

            if (benchmark == null)
            {
                Console.WriteLine($"'{id}' is not a valid benchmark, please try: ");

                var longestId = Benchmarks.Max(x => x.Id.Length);

                foreach (var b in Benchmarks)
                {
                    Console.WriteLine($" * {b.Id.PadRight(longestId)}: {b.Name}");
                }
            }
            else
            {
                const int numRuns = 3;

                try
                {
                    var elapsed = 0d;
                    var count = 0L;

                    benchmark.Initialize();

                    for (var run = 0; run < numRuns; run++)
                    {
                        try
                        {
                            benchmark.RunInitialize();

                            var watch = Stopwatch.StartNew();

                            count += benchmark.Run();

                            watch.Stop();

                            elapsed += watch.ElapsedMilliseconds;
                        }
                        finally
                        {
                            benchmark.RunCleanup();
                        }
                    }

                    var totalElapsed = TimeSpan.FromMilliseconds(elapsed);
                    var totalPerSecond = Math.Round(count / totalElapsed.TotalSeconds, 2);

                    Console.WriteLine($"{benchmark.Name} completed after {totalElapsed}, {totalPerSecond} items/s");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Benchmark failed with '{e.Message}'");
                }
                finally
                {
                    benchmark.Cleanup();
                }
            }
        }
    }
}
