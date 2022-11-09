// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Indexes;

namespace Squidex.Domain.Apps.Entities.Rules.Queries;

public sealed class RuleQueryService : IRuleQueryService
{
    private static readonly List<IEnrichedRuleEntity> EmptyResults = new List<IEnrichedRuleEntity>();
    private readonly IRulesIndex rulesIndex;
    private readonly IRuleEnricher ruleEnricher;

    public RuleQueryService(IRulesIndex rulesIndex, IRuleEnricher ruleEnricher)
    {
        this.rulesIndex = rulesIndex;
        this.ruleEnricher = ruleEnricher;
    }

    public async Task<IReadOnlyList<IEnrichedRuleEntity>> QueryAsync(Context context,
        CancellationToken ct = default)
    {
        var rules = await rulesIndex.GetRulesAsync(context.App.Id, ct);

        if (rules.Count > 0)
        {
            var enriched = await ruleEnricher.EnrichAsync(rules, context, ct);

            return enriched;
        }

        return EmptyResults;
    }
}
