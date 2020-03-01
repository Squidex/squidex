// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class ApiStatsSummary
    {
        public long TotalCalls { get; }

        public long TotalBytes { get; }

        public double AverageElapsedMs { get; }

        public ApiStatsSummary(long totalCalls, double averageElapsedMs, long totalBytes)
        {
            TotalCalls = totalCalls;
            TotalBytes = totalBytes;

            AverageElapsedMs = averageElapsedMs;
        }
    }
}
