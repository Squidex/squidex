// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.UsageTracking
{
    public interface IUsageTracker
    {
        Task TrackAsync(string key, string category, double weight, double elapsedMs);

        Task<long> GetMonthlyCallsAsync(string key, DateTime date);

        Task<IReadOnlyDictionary<string, IReadOnlyList<DateUsage>>> QueryAsync(string key, DateTime fromDate, DateTime toDate);
    }
}
