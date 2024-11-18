// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Indexes;

namespace Squidex.Domain.Apps.Entities.Rules.Queries;

public sealed class RuleQueryService(IRulesIndex rulesIndex, IRuleEnricher ruleEnricher) : IRuleQueryService
{
    private static readonly List<EnrichedRule> EmptyResults = [];

    public async Task<IReadOnlyList<EnrichedRule>> QueryAsync(Context context,
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
