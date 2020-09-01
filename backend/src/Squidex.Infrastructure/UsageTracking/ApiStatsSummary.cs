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

        public long MonthCalls { get; }

        public long MonthBytes { get; }

        public double AverageElapsedMs { get; }

        public ApiStatsSummary(double averageElapsedMs, long totalCalls, long totalBytes, long monthCalls, long monthBytes)
        {
            TotalCalls = totalCalls;
            TotalBytes = totalBytes;

            MonthCalls = monthCalls;
            MonthBytes = monthBytes;

            AverageElapsedMs = averageElapsedMs;
        }
    }
}
