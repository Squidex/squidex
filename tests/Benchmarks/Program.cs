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
        private static readonly List<(string Name, Benchmark Benchmark)> Benchmarks = new Benchmark[]
        {
            new AppendToEventStore(),
            new AppendToEventStoreWithManyWriters(),
            new HandleEvents(),
            new HandleEventsWithManyWriters(),
            new ReadSchemaState()
        }.Select(x => (x.GetType().Name, x)).ToList();

        public static void Main(string[] args)
        {
            var name = "ReadSchemaState";

            var selected = Benchmarks.Find(x => x.Name == name);

            if (selected.Benchmark == null)
            {
                Console.WriteLine($"'{name}' is not a valid benchmark, please try: ");

                foreach (var b in Benchmarks)
                {
                    Console.WriteLine($" * {b.Name}");
                }
            }
            else
            {
                const int numRuns = 3;

                try
                {
                    var elapsed = 0d;
                    var count = 0L;

                    Console.WriteLine($"{selected.Name}: Initialized");

                    selected.Benchmark.Initialize();

                    for (var run = 0; run < numRuns; run++)
                    {
                        try
                        {
                            selected.Benchmark.RunInitialize();

                            var watch = Stopwatch.StartNew();

                            count += selected.Benchmark.Run();

                            watch.Stop();

                            elapsed += watch.ElapsedMilliseconds;

                            Console.WriteLine($"{selected.Name}: Run {run + 1} finished");
                        }
                        finally
                        {
                            selected.Benchmark.RunCleanup();
                        }
                    }

                    var averageElapsed = TimeSpan.FromMilliseconds(elapsed / numRuns);
                    var averageSeconds = Math.Round(count / (numRuns * averageElapsed.TotalSeconds), 2);

                    Console.WriteLine($"{selected.Name}: Completed after {averageElapsed}, {averageSeconds} items/s");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Benchmark failed with '{e.Message}'");
                }
                finally
                {
                    selected.Benchmark.Cleanup();
                }
            }

            Console.ReadLine();
        }
    }
}
