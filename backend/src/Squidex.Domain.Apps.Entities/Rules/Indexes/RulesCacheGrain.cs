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
        private readonly HashSet<DomainId> ruleIds = new HashSet<DomainId>();
        private bool isLoaded;

        public RulesCacheGrain(IGrainIdentity grainIdentity, IRuleRepository ruleRepository)
            : base(grainIdentity)
        {
            this.ruleRepository = ruleRepository;
        }

        public override Task OnActivateAsync()
        {
            return GetRuleIdsAsync();
        }

        public async Task<IReadOnlyCollection<DomainId>> GetRuleIdsAsync()
        {
            if (!isLoaded)
            {
                var loaded = await ruleRepository.QueryIdsAsync(Key);

                foreach (var id in loaded)
                {
                    ruleIds.Add(id);
                }

                isLoaded = true;
            }

            return ruleIds;
        }

        public Task AddAsync(DomainId id)
        {
            ruleIds.Add(id);

            return Task.CompletedTask;
        }

        public async Task RemoveAsync(DomainId id)
        {
            await GetRuleIdsAsync();

            ruleIds.Remove(id);
        }
    }
}
