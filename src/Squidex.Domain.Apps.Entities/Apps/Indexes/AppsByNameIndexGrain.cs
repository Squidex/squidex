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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByNameIndexGrain : GrainOfString, IAppsByNameIndex
    {
        private readonly IStore<string> store;
        private readonly HashSet<Guid> reservedIds = new HashSet<Guid>();
        private readonly HashSet<string> reservedNames = new HashSet<string>();
        private IPersistence<State> persistence;
        private State state = new State();

        [CollectionName("Index_AppsByName")]
        public sealed class State
        {
            public Dictionary<string, Guid> Apps { get; set; } = new Dictionary<string, Guid>();
        }

        public AppsByNameIndexGrain(IStore<string> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public override Task OnActivateAsync(string key)
        {
            persistence = store.WithSnapshots<AppsByNameIndexGrain, State, string>(key, s =>
            {
                state = s;
            });

            return persistence.ReadAsync();
        }

        public Task RebuildAsync(Dictionary<string, Guid> apps)
        {
            state = new State { Apps = apps };

            return persistence.WriteSnapshotAsync(state);
        }

        public Task<bool> ReserveAppAsync(Guid appId, string name)
        {
            var canReserve =
                !state.Apps.ContainsKey(name) &&
                !state.Apps.Any(x => x.Value == appId) &&
                !reservedIds.Contains(appId) &&
                !reservedNames.Contains(name);

            if (canReserve)
            {
                reservedIds.Add(appId);
                reservedNames.Add(name);
            }

            return Task.FromResult(canReserve);
        }

        public Task RemoveReservationAsync(Guid appId, string name)
        {
            reservedIds.Remove(appId);
            reservedNames.Remove(name);

            return TaskHelper.Done;
        }

        public Task AddAppAsync(Guid appId, string name)
        {
            state.Apps[name] = appId;

            reservedIds.Remove(appId);
            reservedNames.Remove(name);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task RemoveAppAsync(Guid appId)
        {
            var name = state.Apps.FirstOrDefault(x => x.Value == appId).Key;

            if (!string.IsNullOrWhiteSpace(name))
            {
                state.Apps.Remove(name);

                reservedIds.Remove(appId);
                reservedNames.Remove(name);
            }

            return persistence.WriteSnapshotAsync(state);
        }

        public Task<Guid> GetAppIdAsync(string appName)
        {
            state.Apps.TryGetValue(appName, out var appId);

            return Task.FromResult(appId);
        }

        public Task<List<Guid>> GetAppIdsAsync()
        {
            return Task.FromResult(state.Apps.Values.ToList());
        }
    }
}
