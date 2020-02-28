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
        Task TrackAsync(DateTime date, string key, string? category, double weight, long elapsed, long bytes);

        Task<long> GetMonthlyWeightAsync(string key, DateTime date);

        Task<(ApiStats Summary, Dictionary<string, List<(DateTime Date, ApiStats Stats)>> Details)> QueryAsync(string key, DateTime fromDate, DateTime toDate);
    }
}
