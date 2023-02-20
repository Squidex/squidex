// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IRuleUsageTracker
{
    Task<IReadOnlyList<RuleStats>> QueryByAppAsync(DomainId appId, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct = default);

    Task<IReadOnlyList<RuleStats>> QueryByTeamAsync(DomainId teamId, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<DomainId, RuleCounters>> GetTotalByAppAsync(DomainId appId,
        CancellationToken ct = default);

    Task TrackAsync(DomainId appId, DomainId ruleId, DateOnly date, int created, int succeeded, int failed,
        CancellationToken ct = default);

    Task DeleteUsageAsync(DomainId appId,
        CancellationToken ct = default);
}

public record struct RuleStats(DateOnly Date, RuleCounters Counters);

public record struct RuleCounters(long TotalCreated, long TotalSucceeded, long TotalFailed);
