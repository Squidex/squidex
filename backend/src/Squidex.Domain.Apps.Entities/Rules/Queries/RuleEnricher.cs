// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules.Queries;

public sealed class RuleEnricher : IRuleEnricher
{
    private readonly IRuleUsageTracker ruleUsageTracker;
    private readonly IRequestCache requestCache;

    public RuleEnricher(IRuleUsageTracker ruleUsageTracker, IRequestCache requestCache)
    {
        this.ruleUsageTracker = ruleUsageTracker;
        this.requestCache = requestCache;
    }

    public async Task<IEnrichedRuleEntity> EnrichAsync(IRuleEntity rule, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(rule);

        var enriched = await EnrichAsync(Enumerable.Repeat(rule, 1), context, ct);

        return enriched[0];
    }

    public async Task<IReadOnlyList<IEnrichedRuleEntity>> EnrichAsync(IEnumerable<IRuleEntity> rules, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(rules);
        Guard.NotNull(context);

        using (Telemetry.Activities.StartActivity("RuleEnricher/EnrichAsync"))
        {
            var results = new List<RuleEntity>();

            foreach (var rule in rules)
            {
                var result = SimpleMapper.Map(rule, new RuleEntity());

                results.Add(result);
            }

            // Sometimes we just want to skip this for performance reasons.
            var enrichCacheKeys = !context.ShouldSkipCacheKeys();

            foreach (var group in results.GroupBy(x => x.AppId.Id))
            {
                var statistics = await ruleUsageTracker.GetTotalByAppAsync(group.Key, ct);

                foreach (var rule in group)
                {
                    if (statistics.TryGetValue(rule.Id, out var statistic))
                    {
                        rule.NumFailed = statistic.TotalFailed;
                        rule.NumSucceeded = statistic.TotalSucceeded;
                    }

                    if (enrichCacheKeys)
                    {
                        requestCache.AddDependency(rule.UniqueId, rule.Version);
                        requestCache.AddDependency(rule.NumFailed);
                        requestCache.AddDependency(rule.NumSucceeded);
                    }
                }
            }

            return results;
        }
    }
}
