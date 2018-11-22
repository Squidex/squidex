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
        Task TrackAsync(Guid appId, string category, double weight, double elapsedMs);

        Task<long> GetMonthlyCallsAsync(Guid appId, DateTime date);

        Task<IReadOnlyDictionary<string, IReadOnlyList<DateUsage>>> QueryAsync(Guid appId, DateTime fromDate, DateTime toDate);
    }
}
