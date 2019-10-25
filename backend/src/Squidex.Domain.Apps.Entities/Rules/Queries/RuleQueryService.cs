﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Queries
{
    public sealed class RuleQueryService : IRuleQueryService
    {
        private readonly IRulesIndex rulesIndex;
        private readonly IRuleEnricher ruleEnricher;

        public RuleQueryService(IRulesIndex rulesIndex, IRuleEnricher ruleEnricher)
        {
            Guard.NotNull(rulesIndex, nameof(rulesIndex));
            Guard.NotNull(ruleEnricher, nameof(ruleEnricher));

            this.rulesIndex = rulesIndex;
            this.ruleEnricher = ruleEnricher;
        }

        public async Task<IReadOnlyList<IEnrichedRuleEntity>> QueryAsync(Context context)
        {
            var rules = await rulesIndex.GetRulesAsync(context.App.Id);

            var enriched = await ruleEnricher.EnrichAsync(rules, context);

            return enriched;
        }
    }
}
