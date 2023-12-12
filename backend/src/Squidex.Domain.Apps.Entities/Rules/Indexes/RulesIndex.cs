// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes;

public sealed class RulesIndex : IRulesIndex
{
    private readonly IRuleRepository ruleRepository;

    public RulesIndex(IRuleRepository ruleRepository)
    {
        this.ruleRepository = ruleRepository;
    }

    public async Task<List<Rule>> GetRulesAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("RulesIndex/GetRulesAsync"))
        {
            var rules = await ruleRepository.QueryAllAsync(appId, ct);

            return rules.Where(IsValid).ToList();
        }
    }

    private static bool IsValid(Rule? rule)
    {
        return rule is { Version: > EtagVersion.Empty, IsDeleted: false };
    }
}
