// ==========================================================================
//  Usage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class Usage
    {
        public readonly double Count;
        public readonly double ElapsedMs;

        public Usage(double elapsed, double count)
        {
            ElapsedMs = elapsed;

            Count = count;
        }

        public Usage Add(double elapsed, double weight)
        {
            return new Usage(ElapsedMs + elapsed, Count + weight);
        }
    }
}
