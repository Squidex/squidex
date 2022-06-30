// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesIndex : IRulesIndex
    {
        private readonly IRuleRepository ruleRepository;

        public RulesIndex(IRuleRepository ruleRepository)
        {
            this.ruleRepository = ruleRepository;
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(DomainId appId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("RulesIndex/GetRulesAsync"))
            {
                return await ruleRepository.QueryAllAsync(appId, ct);
            }
        }
    }
}
