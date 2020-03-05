// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<IndexingBenchmarks>();
        }
    }
}
