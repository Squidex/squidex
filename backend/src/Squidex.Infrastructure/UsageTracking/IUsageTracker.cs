// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.UsageTracking
{
    public interface IUsageTracker
    {
        Task TrackAsync(DateTime date, string key, string? category, Counters counters,
            CancellationToken ct = default);

        Task<Counters> GetForMonthAsync(string key, DateTime date, string? category,
            CancellationToken ct = default);

        Task<Counters> GetAsync(string key, DateTime fromDate, DateTime toDate, string? category,
            CancellationToken ct = default);

        Task<Dictionary<string, List<(DateTime, Counters)>>> QueryAsync(string key, DateTime fromDate, DateTime toDate,
            CancellationToken ct = default);

        Task DeleteAsync(string key,
            CancellationToken ct = default);
    }
}
