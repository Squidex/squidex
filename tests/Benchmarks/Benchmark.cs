// ==========================================================================
//  IBenchmark.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Benchmarks
{
    public abstract class Benchmark
    {
        public virtual void Initialize()
        {
        }

        public virtual void RunInitialize()
        {
        }

        public virtual void RunCleanup()
        {
        }

        public virtual void Cleanup()
        {
        }

        public abstract long Run();
    }
}
