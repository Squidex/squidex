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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesByAppIndexGrain : GrainOfGuid, IRulesByAppIndexGrain
    {
        private readonly IGrainState<GrainState> state;

        [CollectionName("Index_RulesByApp")]
        public sealed class GrainState
        {
            public HashSet<Guid> Rules { get; set; } = new HashSet<Guid>();
        }

        public RulesByAppIndexGrain(IGrainState<GrainState> state)
        {
            Guard.NotNull(state, nameof(state));

            this.state = state;
        }

        public Task ClearAsync()
        {
            return state.ClearAsync();
        }

        public Task RebuildAsync(HashSet<Guid> rules)
        {
            state.Value = new GrainState { Rules = rules };

            return state.WriteAsync();
        }

        public Task AddRuleAsync(Guid ruleId)
        {
            state.Value.Rules.Add(ruleId);

            return state.WriteAsync();
        }

        public Task RemoveRuleAsync(Guid ruleId)
        {
            state.Value.Rules.Remove(ruleId);

            return state.WriteAsync();
        }

        public Task<List<Guid>> GetRuleIdsAsync()
        {
            return Task.FromResult(state.Value.Rules.ToList());
        }
    }
}
