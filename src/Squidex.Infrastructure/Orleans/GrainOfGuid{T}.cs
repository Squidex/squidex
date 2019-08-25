// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainOfGuid<T> : GrainOfGuid where T : class, new()
    {
        private readonly IStore<Guid> store;
        private IPersistence<T> persistence;

        protected T State { get; set; } = new T();

        protected IPersistence<T> Persistence
        {
            get { return persistence; }
        }

        protected GrainOfGuid(IStore<Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        protected sealed override Task OnLoadAsync(Guid key)
        {
            persistence = store.WithSnapshots(GetType(), key, new HandleSnapshot<T>(ApplyState));

            return persistence.ReadAsync();
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
