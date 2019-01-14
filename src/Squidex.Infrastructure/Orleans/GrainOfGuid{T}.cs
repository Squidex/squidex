// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainOfGuid<T> : Grain where T : class, new()
    {
        private readonly IStore<Guid> store;
        private IPersistence<T> persistence;

        protected T State { get; set; } = new T();

        public Guid Key { get; private set; }

        protected IPersistence<T> Persistence
        {
            get { return persistence; }
        }

        protected GrainOfGuid(IStore<Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public sealed override Task OnActivateAsync()
        {
            return ActivateAsync(this.GetPrimaryKey());
        }

        public async Task ActivateAsync(Guid key)
        {
            Key = key;

            persistence = store.WithSnapshots(GetType(), key, new HandleSnapshot<T>(ApplyState));

            await persistence.ReadAsync();

            await OnActivateAsync(key);
        }

        protected virtual Task OnActivateAsync(Guid key)
        {
            return TaskHelper.Done;
        }

        private void ApplyState(T state)
        {
            State = state;
        }

        public Task ClearStateAsync()
        {
            State = new T();

            return persistence.DeleteAsync();
        }

        protected Task WriteStateAsync()
        {
            return persistence.WriteSnapshotAsync(State);
        }
    }
}
