// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.UsageTracking
{
    public interface IApiUsageTracker
    {
        Task TrackAsync(DateTime date, string key, string? category, double weight, long elapsedMs, long bytes);

        Task<long> GetMonthCallsAsync(string key, DateTime date, string? category);

        Task<long> GetMonthBytesAsync(string key, DateTime date, string? category);

        Task<(ApiStatsSummary, Dictionary<string, List<ApiStats>> Details)> QueryAsync(string key, DateTime fromDate, DateTime toDate);
    }
}
