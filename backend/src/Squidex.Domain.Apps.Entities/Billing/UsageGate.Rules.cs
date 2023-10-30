// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed partial class UsageGate : IRuleUsageTracker
{
    public static class RulesKeys
    {
        public const string TotalCreated = nameof(RuleCounters.TotalCreated);
        public const string TotalSucceeded = nameof(RuleCounters.TotalSucceeded);
        public const string TotalFailed = nameof(RuleCounters.TotalFailed);
    }

    Task IRuleUsageTracker.DeleteUsageAsync(DomainId appId,
        CancellationToken ct)
    {
        // Use a well defined prefix query for the deletion to improve performance.
        return usageTracker.DeleteAsync(AppRulesKey(appId), ct);
    }

    Task<IReadOnlyList<RuleStats>> IRuleUsageTracker.QueryByAppAsync(DomainId appId, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct)
    {
        return QueryForRulesAsync(AppRulesKey(appId), fromDate, toDate, ct);
    }

    Task<IReadOnlyList<RuleStats>> IRuleUsageTracker.QueryByTeamAsync(DomainId appId, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct)
    {
        return QueryForRulesAsync(TeamRulesKey(appId), fromDate, toDate, ct);
    }

    async Task<IReadOnlyDictionary<DomainId, RuleCounters>> IRuleUsageTracker.GetTotalByAppAsync(DomainId appId,
        CancellationToken ct)
    {
        var result = new Dictionary<DomainId, RuleCounters>();

        var counters = await usageTracker.QueryAsync(AppRulesKey(appId), SummaryDate, SummaryDate, ct);

        foreach (var (category, byCategory) in counters)
        {
            if (byCategory.Count > 0)
            {
                result[DomainId.Create(category)] = GetRuleCounters(byCategory[0].Item2);
            }
        }

        return result;
    }

    private async Task<IReadOnlyList<RuleStats>> QueryForRulesAsync(string key, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct)
    {
        var result = new List<RuleStats>();

        var usages = await usageTracker.QueryAsync(key, fromDate, toDate, ct);

        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            var aggregated = default(RuleCounters);

            foreach (var (_, byCategory) in usages)
            {
                foreach (var (counterDate, counters) in byCategory)
                {
                    if (counterDate == date)
                    {
                        var currentCounters = GetRuleCounters(counters);

                        aggregated.TotalCreated += currentCounters.TotalCreated;
                        aggregated.TotalSucceeded += currentCounters.TotalSucceeded;
                        aggregated.TotalFailed += currentCounters.TotalFailed;
                    }
                }
            }

            result.Add(new RuleStats(date, aggregated));
        }

        return result;
    }

    async Task IRuleUsageTracker.TrackAsync(DomainId appId, DomainId ruleId, DateOnly date, int created, int succeeded, int failed,
        CancellationToken ct)
    {
        var counters = new Counters
        {
            [RulesKeys.TotalCreated] = created,
            [RulesKeys.TotalSucceeded] = succeeded,
            [RulesKeys.TotalFailed] = failed
        };

        var appKey = AppRulesKey(appId);

        var tasks = new List<Task>
        {
            usageTracker.TrackAsync(SummaryDate, appKey, ruleId.ToString(), counters, ct)
        };

        if (date != default)
        {
            tasks.Add(usageTracker.TrackAsync(date, appKey, ruleId.ToString(), counters, ct));
        }

        var (_, _, teamId) = await GetPlanForAppAsync(appId, true, ct);

        if (teamId != null)
        {
            var teamKey = TeamRulesKey(teamId.Value);

            tasks.Add(usageTracker.TrackAsync(SummaryDate, teamKey, appId.ToString(), counters, ct));

            if (date != default)
            {
                tasks.Add(usageTracker.TrackAsync(date, teamKey, appId.ToString(), counters, ct));
            }
        }

        await Task.WhenAll(tasks);
    }

    private static RuleCounters GetRuleCounters(Counters counters)
    {
        return new RuleCounters(
            counters.GetInt64(RulesKeys.TotalCreated),
            counters.GetInt64(RulesKeys.TotalSucceeded),
            counters.GetInt64(RulesKeys.TotalFailed));
    }

    private static string AppRulesKey(DomainId appId)
    {
        return $"{appId}_Rules";
    }

    private static string TeamRulesKey(DomainId teamId)
    {
        return $"{teamId}_TeamRules";
    }
}
