// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class ApiStats
    {
        public DateTime Date { get; }

        public long TotalCalls { get; }

        public long TotalBytes { get; }

        public double AverageElapsedMs { get; }

        public ApiStats(DateTime date, long totalCalls, double averageElapsedMs, long totalBytes)
        {
            Date = date;

            TotalCalls = totalCalls;
            TotalBytes = totalBytes;

            AverageElapsedMs = averageElapsedMs;
        }
    }
}
