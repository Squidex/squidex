// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public class IdsIndexGrain<TState, T> : Grain, IIdsIndexGrain<T> where TState : IdsIndexState<T>, new()
    {
        private readonly IGrainState<TState> state;

        public IdsIndexGrain(IGrainState<TState> state)
        {
            this.state = state;
        }

        public Task<long> CountAsync()
        {
            return Task.FromResult<long>(state.Value.Ids.Count);
        }

        public Task RebuildAsync(HashSet<T> ids)
        {
            state.Value = new TState { Ids = ids };

            return state.WriteAsync();
        }

        public Task AddAsync(T id)
        {
            state.Value.Ids.Add(id);

            return state.WriteAsync();
        }

        public Task RemoveAsync(T id)
        {
            state.Value.Ids.Remove(id);

            return state.WriteAsync();
        }

        public Task ClearAsync()
        {
            return state.ClearAsync();
        }

        public Task<List<T>> GetIdsAsync()
        {
            return Task.FromResult(state.Value.Ids.ToList());
        }
    }
}
