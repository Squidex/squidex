// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Areas.Api.Controllers.Statistics.Models;

public sealed class CallsUsagePerDateDto
{
    /// <summary>
    /// The date when the usage was tracked.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The total number of API calls.
    /// </summary>
    public long TotalCalls { get; set; }

    /// <summary>
    /// The total number of bytes transferred.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// The average duration in milliseconds.
    /// </summary>
    public double AverageElapsedMs { get; set; }

    public static CallsUsagePerDateDto FromDomain(ApiStats stats)
    {
        return SimpleMapper.Map(stats, new CallsUsagePerDateDto());
    }
}
