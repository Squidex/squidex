// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class DateUsage
    {
        public DateTime Date { get; }

        public long TotalCount { get; }

        public long TotalElapsedMs { get; }

        public DateUsage(DateTime date, long totalCount, long totalElapsedMs)
        {
            Date = date;

            TotalCount = totalCount;
            TotalElapsedMs = totalElapsedMs;
        }
    }
}
