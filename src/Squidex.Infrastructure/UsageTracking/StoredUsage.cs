// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class StoredUsage
    {
        public string Category { get; }

        public DateTime Date { get; }

        public long TotalCount { get; }

        public long TotalElapsedMs { get; }

        public StoredUsage(string category, DateTime date, long totalCount, long totalElapsedMs)
        {
            Category = category;

            Date = date;

            TotalCount = totalCount;
            TotalElapsedMs = totalElapsedMs;
        }
    }
}
