// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class ApiStats
    {
        public long TotalCalls { get; }

        public long TotalBytes { get; }

        public double AverageElapsed { get; }

        public ApiStats(long totalCalls, double averageElapsed, long totalBytes)
        {
            TotalCalls = totalCalls;
            TotalBytes = totalBytes;

            AverageElapsed = averageElapsed;
        }
    }
}
