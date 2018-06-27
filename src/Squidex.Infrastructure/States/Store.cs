// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public sealed class Store<TKey> : IStore<TKey>
    {
        private readonly IServiceProvider services;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;

        public Store(
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IServiceProvider services,
            IStreamNameResolver streamNameResolver)
        {
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.services = services;
            this.streamNameResolver = streamNameResolver;
        }

        public IPersistence<TState> WithSnapshots<TState>(Type owner, TKey key, Func<TState, Task> applySnapshot)
        {
            return CreatePersistence(owner, key, PersistenceMode.Snapshots, applySnapshot, null);
        }

        public IPersistence<TState> WithSnapshotsAndEventSourcing<TState>(Type owner, TKey key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return CreatePersistence(owner, key, PersistenceMode.SnapshotsAndEventSourcing, applySnapshot, applyEvent);
        }

        public IPersistence WithEventSourcing(Type owner, TKey key, Func<Envelope<IEvent>, Task> applyEvent)
        {
            Guard.NotNull(key, nameof(key));

            var snapshotStore = (ISnapshotStore<object, TKey>)services.GetService(typeof(ISnapshotStore<object, TKey>));

            return new Persistence<TKey>(key, owner, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, applyEvent);
        }

        private IPersistence<TState> CreatePersistence<TState>(Type owner, TKey key, PersistenceMode mode, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            Guard.NotNull(key, nameof(key));

            var snapshotStore = (ISnapshotStore<TState, TKey>)services.GetService(typeof(ISnapshotStore<TState, TKey>));

            return new Persistence<TState, TKey>(key, owner, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, mode, applySnapshot, applyEvent);
        }

        public Task ClearSnapshotsAsync<TState>()
        {
            var snapshotStore = (ISnapshotStore<TState, TKey>)services.GetService(typeof(ISnapshotStore<TState, TKey>));

            return snapshotStore.ClearAsync();
        }
    }
}
