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

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByUserIndexGrain : GrainOfString, IAppsByUserIndex
    {
        private readonly IStore<string> store;
        private IPersistence<State> persistence;
        private State state = new State();

        [CollectionName("Index_AppsByUser")]
        public sealed class State
        {
            public HashSet<Guid> Apps { get; set; } = new HashSet<Guid>();
        }

        public AppsByUserIndexGrain(IStore<string> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public override Task OnActivateAsync(string key)
        {
            persistence = store.WithSnapshots<AppsByUserIndexGrain, State, string>(key, s =>
            {
                state = s;
            });

            return persistence.ReadAsync();
        }

        public Task RebuildAsync(HashSet<Guid> apps)
        {
            state = new State { Apps = apps };

            return persistence.WriteSnapshotAsync(state);
        }

        public Task AddAppAsync(Guid appId)
        {
            state.Apps.Add(appId);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task RemoveAppAsync(Guid appId)
        {
            state.Apps.Remove(appId);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task<List<Guid>> GetAppIdsAsync()
        {
            return Task.FromResult(state.Apps.ToList());
        }
    }
}
