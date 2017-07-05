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
        string Id { get; }

        string Name { get; }

        void Initialize();

        void RunInitialize();

        long Run();

        void RunCleanup();

        void Cleanup();
    }
}
