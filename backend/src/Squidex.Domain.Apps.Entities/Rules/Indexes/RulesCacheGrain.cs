// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Concurrency;
using Orleans.Core;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    [Reentrant]
    public sealed class RulesCacheGrain : GrainBase, IRulesCacheGrain
    {
        private readonly IRuleRepository ruleRepository;
        private List<DomainId>? ruleIds;

        public RulesCacheGrain(IGrainIdentity grainIdentity, IRuleRepository ruleRepository)
            : base(grainIdentity)
        {
            this.ruleRepository = ruleRepository;
        }

        public async Task<IReadOnlyCollection<DomainId>> GetRuleIdsAsync()
        {
            var ids = ruleIds;

            if (ids == null)
            {
                ids = await ruleRepository.QueryIdsAsync(Key);

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
