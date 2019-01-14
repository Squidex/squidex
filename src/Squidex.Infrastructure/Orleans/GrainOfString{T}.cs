// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public abstract class GrainOfString<T> : Grain where T : class, new()
    {
        private readonly IStore<string> store;
        private IPersistence<T> persistence;

        public string Key { get; set; }

        protected T State { get; set; } = new T();

        protected IPersistence<T> Persistence
        {
            get { return persistence; }
        }

        protected GrainOfString(IStore<string> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public sealed override Task OnActivateAsync()
        {
            return ActivateAsync(this.GetPrimaryKeyString());
        }

        public async Task ActivateAsync(string key)
        {
            Key = key;

            persistence = store.WithSnapshots(GetType(), key, new HandleSnapshot<T>(ApplyState));

            await persistence.ReadAsync();

            await OnActivateAsync(key);
        }

        protected virtual Task OnActivateAsync(string key)
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
