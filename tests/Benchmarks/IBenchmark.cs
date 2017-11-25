// ==========================================================================
//  IBenchmark.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Benchmarks
{
    public interface IBenchmark
    {
        void RunInitialize();

        long Run();

        void RunCleanup();
    }
}
