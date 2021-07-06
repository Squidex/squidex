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
    public class UniqueNameIndexGrain<TState, T> : Grain, IUniqueNameIndexGrain<T> where TState : UniqueNameIndexState<T>, new()
    {
        private readonly Dictionary<string, (string Name, T Id)> reservations = new Dictionary<string, (string Name, T Id)>();
        private readonly IGrainState<TState> state;

        public UniqueNameIndexGrain(IGrainState<TState> state)
        {
            this.state = state;
        }

        public Task<long> CountAsync()
        {
            return Task.FromResult<long>(state.Value.Names.Count);
        }

        public Task ClearAsync()
        {
            reservations.Clear();

            return state.ClearAsync();
        }

        public Task RebuildAsync(Dictionary<string, T> names)
        {
            state.Value = new TState { Names = names };

            return state.WriteAsync();
        }

        public Task<string?> ReserveAsync(T id, string name)
        {
            string? token = null;

            if (!IsInUse(name) && !IsReserved(name))
            {
                token = RandomHash.Simple();

                reservations.Add(token, (name, id));
            }

            return Task.FromResult(token);
        }

        public async Task<bool> AddAsync(string? token)
        {
            token ??= string.Empty;

            if (reservations.TryGetValue(token, out var reservation))
            {
                state.Value.Names.Add(reservation.Name, reservation.Id);

                await state.WriteAsync();

                reservations.Remove(token);

                return true;
            }

            return false;
        }

        public Task RemoveReservationAsync(string? token)
        {
            reservations.Remove(token ?? string.Empty);

            return Task.CompletedTask;
        }

        public async Task RemoveAsync(T id)
        {
            var name = state.Value.Names.FirstOrDefault(x => Equals(x.Value, id)).Key;

            if (name != null)
            {
                state.Value.Names.Remove(name);

                await state.WriteAsync();
            }
        }

        public Task<List<T>> GetIdsAsync(string[] names)
        {
            var result = new List<T>();

            if (names != null)
            {
                foreach (var name in names)
                {
                    if (state.Value.Names.TryGetValue(name, out var id))
                    {
                        result.Add(id);
                    }
                }
            }

            return Task.FromResult(result);
        }

        public Task<T> GetIdAsync(string name)
        {
            state.Value.Names.TryGetValue(name, out var id);

            return Task.FromResult(id!);
        }

        public Task<List<T>> GetIdsAsync()
        {
            return Task.FromResult(state.Value.Names.Values.ToList());
        }

        private bool IsInUse(string name)
        {
            return state.Value.Names.ContainsKey(name);
        }

        private bool IsReserved(string name)
        {
            return reservations.Values.Any(x => x.Name == name);
        }
    }
}
