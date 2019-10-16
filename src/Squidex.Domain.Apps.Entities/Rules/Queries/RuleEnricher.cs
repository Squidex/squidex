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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules.Queries
{
    public sealed class RuleEnricher : IRuleEnricher
    {
        private readonly IRuleEventRepository ruleEventRepository;

        public RuleEnricher(IRuleEventRepository ruleEventRepository)
        {
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));

            this.ruleEventRepository = ruleEventRepository;
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
                        var statistic = statistics.FirstOrDefault(x => x.RuleId == rule.Id);

                        if (statistic != null)
                        {
                            rule.LastExecuted = statistic.LastExecuted;
                            rule.NumFailed = statistic.NumFailed;
                            rule.NumSucceeded = statistic.NumSucceeded;

                            rule.CacheDependencies = new HashSet<object>
                            {
                                statistic.LastExecuted
                            };
                        }
                    }
                }

                return results;
            }
        }
    }
}
