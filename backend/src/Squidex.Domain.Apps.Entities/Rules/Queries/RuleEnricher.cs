// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
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

    public async Task<EnrichedRule> EnrichAsync(Rule rule, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(rule);

        var enriched = await EnrichAsync(Enumerable.Repeat(rule, 1), context, ct);

        return enriched[0];
    }

    public async Task<IReadOnlyList<EnrichedRule>> EnrichAsync(IEnumerable<Rule> rules, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(rules);
        Guard.NotNull(context);

        using (Telemetry.Activities.StartActivity("RuleEnricher/EnrichAsync"))
        {
            var results = new List<EnrichedRule>();

            // Sometimes we just want to skip this for performance reasons.
            var enrichCacheKeys = !context.NoCacheKeys();

            foreach (var group in rules.GroupBy(x => x.AppId.Id))
            {
                var statistics = await ruleUsageTracker.GetTotalByAppAsync(group.Key, ct);

                foreach (var rule in group)
                {
                    var result = SimpleMapper.Map(rule, new EnrichedRule());

                    if (statistics.TryGetValue(rule.Id, out var statistic))
                    {
                        result = result with
                        {
                            NumFailed = statistic.TotalFailed,
                            NumSucceeded = statistic.TotalSucceeded
                        };
                    }

                    if (enrichCacheKeys)
                    {
                        requestCache.AddDependency(result.UniqueId, result.Version);
                        requestCache.AddDependency(result.NumFailed);
                        requestCache.AddDependency(result.NumSucceeded);
                    }

                    results.Add(result);
                }
            }

            return results;
        }
    }
}
