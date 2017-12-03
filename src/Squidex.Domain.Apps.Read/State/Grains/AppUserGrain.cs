// ==========================================================================
//  AppUserGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public sealed class AppUserGrain : IStatefulObject
    {
        private IPersistence<AppUserGrainState> persistence;
        private AppUserGrainState state = new AppUserGrainState();

        public Task ActivateAsync(string key, IStore store)
        {
            persistence = store.WithSnapshots<AppUserGrain, AppUserGrainState>(key, s => state = s);

            return persistence.ReadAsync();
        }

        public Task AddAppAsync(string appName)
        {
            state = state.AddApp(appName);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task RemoveAppAsync(string appName)
        {
            state = state.RemoveApp(appName);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task<List<string>> GetAppNamesAsync()
        {
            return Task.FromResult(state.AppNames.ToList());
        }
    }
}
