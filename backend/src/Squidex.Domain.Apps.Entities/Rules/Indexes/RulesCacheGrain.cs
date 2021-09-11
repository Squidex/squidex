// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    [Reentrant]
    public sealed class RulesCacheGrain : GrainOfString, IRulesCacheGrain
    {
        private readonly IRuleRepository ruleRepository;
        private List<DomainId>? ruleIds;

        private DomainId AppId => DomainId.Create(Key);

        public RulesCacheGrain(IRuleRepository ruleRepository)
        {
            this.ruleRepository = ruleRepository;
        }

        public async Task<IReadOnlyCollection<DomainId>> GetRuleIdsAsync()
        {
            var ids = ruleIds;

            if (ids == null)
            {
                ids = await ruleRepository.QueryIdsAsync(AppId);

                ruleIds = ids;
            }

            return ids;
        }

        public Task AddAsync(DomainId id)
        {
            ruleIds?.Add(id);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(DomainId id)
        {
            ruleIds?.Remove(id);

            return Task.CompletedTask;
        }
    }
}
