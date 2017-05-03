// ==========================================================================
//  StoredUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class StoredUsage
    {
        public DateTime Date { get; }

        public long TotalCount { get; }

        public long TotalElapsedMs { get; }

        public StoredUsage(DateTime date, long totalCount, long totalElapsedMs)
        {
            Date = date;

            TotalCount = totalCount;
            TotalElapsedMs = totalElapsedMs;
        }
    }
}
