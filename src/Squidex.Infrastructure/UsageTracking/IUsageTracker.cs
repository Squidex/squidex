// ==========================================================================
//  IUsageTracker.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.UsageTracking
{
    public interface IUsageTracker
    {
        Task TrackAsync(string key, double weight, double elapsedMs);

        Task<long> GetMonthlyCalls(string key, DateTime date);

        Task<IReadOnlyList<StoredUsage>> QueryAsync(string key, DateTime fromDate, DateTime toDate);
    }
}
