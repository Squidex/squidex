// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Reflection;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Rules.Queries
{
    public sealed class RuleEnricher : IRuleEnricher
    {
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IRequestCache requestCache;

        public RuleEnricher(IRuleEventRepository ruleEventRepository, IRequestCache requestCache)
        {
            this.ruleEventRepository = ruleEventRepository;

            this.requestCache = requestCache;
        }

        public async Task<IEnrichedRuleEntity> EnrichAsync(IRuleEntity rule, Context context)
        {
            Guard.NotNull(rule, nameof(rule));

            var enriched = await EnrichAsync(Enumerable.Repeat(rule, 1), context);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedRuleEntity>> EnrichAsync(IEnumerable<IRuleEntity> rules, Context context)
        {
            Guard.NotNull(rules, nameof(rules));
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<RuleEnricher>())
            {
                var results = new List<RuleEntity>();

                foreach (var rule in rules)
                {
                    var result = SimpleMapper.Map(rule, new RuleEntity());

                    results.Add(result);
                }

                foreach (var group in results.GroupBy(x => x.AppId.Id))
                {
                    var statistics = await ruleEventRepository.QueryStatisticsByAppAsync(group.Key);

                    foreach (var rule in group)
                    {
                        requestCache.AddDependency(rule.UniqueId, rule.Version);

                        var statistic = statistics.FirstOrDefault(x => x.RuleId == rule.Id);

                        if (statistic != null)
                        {
                            rule.LastExecuted = statistic.LastExecuted;
                            rule.NumFailed = statistic.NumFailed;
                            rule.NumSucceeded = statistic.NumSucceeded;

                            requestCache.AddDependency(rule.LastExecuted);
                        }
                    }
                }

                return results;
            }
        }
    }
}
