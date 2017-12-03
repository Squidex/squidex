// ==========================================================================
//  MyAppState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.State.Grains;
using Squidex.Infrastructure.States;

namespace Benchmarks.Tests.TestData
{
    public sealed class MyAppState : IStatefulObject
    {
        private IPersistence<AppStateGrainState> persistence;
        private AppStateGrainState state;

        public Task ActivateAsync(string key, IStore store)
        {
            persistence = store.WithSnapshots<MyAppState, AppStateGrainState>(key, s => state = s);

            return persistence.ReadAsync();
        }

        public void SetState(AppStateGrainState state)
        {
            this.state = state;
        }

        public Task WriteStateAsync()
        {
            return persistence.WriteSnapshotAsync(state);
        }

        public Task ReadStateAsync()
        {
            return persistence.ReadAsync();
        }
    }
}
