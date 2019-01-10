// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesByAppIndexGrain : GrainOfGuid<RulesByAppIndexGrain.GrainState>, IRulesByAppIndex
    {
        [CollectionName("Index_RulesByApp")]
        public sealed class GrainState
        {
            public HashSet<Guid> Rules { get; set; } = new HashSet<Guid>();
        }

        public RulesByAppIndexGrain(IStore<Guid> store)
            : base(store)
        {
        }

        public Task ClearAsync()
        {
            return ClearStateAsync();
        }

        public Task RebuildAsync(HashSet<Guid> rules)
        {
            State = new GrainState { Rules = rules };

            return WriteStateAsync();
        }

        public Task AddRuleAsync(Guid ruleId)
        {
            State.Rules.Add(ruleId);

            return WriteStateAsync();
        }

        public Task RemoveRuleAsync(Guid ruleId)
        {
            State.Rules.Remove(ruleId);

            return WriteStateAsync();
        }

        public Task<List<Guid>> GetRuleIdsAsync()
        {
            return Task.FromResult(State.Rules.ToList());
        }
    }
}
