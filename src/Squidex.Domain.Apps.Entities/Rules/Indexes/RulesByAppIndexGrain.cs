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
    public sealed class RulesByAppIndexGrain : GrainOfGuid, IRulesByAppIndex
    {
        private readonly IStore<Guid> store;
        private IPersistence<State> persistence;
        private State state = new State();

        [CollectionName("Index_RulesByApp")]
        public sealed class State
        {
            public HashSet<Guid> Rules { get; set; } = new HashSet<Guid>();
        }

        public RulesByAppIndexGrain(IStore<Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public override Task OnActivateAsync(Guid key)
        {
            persistence = store.WithSnapshots<RulesByAppIndexGrain, State, Guid>(key, s =>
            {
                state = s;
            });

            return persistence.ReadAsync();
        }

        public Task ClearAsync()
        {
            state = new State();

            return persistence.DeleteAsync();
        }

        public Task RebuildAsync(HashSet<Guid> rules)
        {
            state = new State { Rules = rules };

            return persistence.WriteSnapshotAsync(state);
        }

        public Task AddRuleAsync(Guid ruleId)
        {
            state.Rules.Add(ruleId);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task RemoveRuleAsync(Guid ruleId)
        {
            state.Rules.Remove(ruleId);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task<List<Guid>> GetRuleIdsAsync()
        {
            return Task.FromResult(state.Rules.ToList());
        }
    }
}
