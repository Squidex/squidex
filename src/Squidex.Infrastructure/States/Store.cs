// ==========================================================================
//  Store.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    internal sealed class Store<TOwner, TKey> : IStore<TKey>
    {
        private readonly Action invalidate;
        private readonly Action failed;
        private readonly IServiceProvider services;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;

        public Store(
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IServiceProvider services,
            IStreamNameResolver streamNameResolver,
            Action invalidate = null,
            Action failed = null)
        {
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.failed = failed;
            this.invalidate = invalidate;
            this.services = services;
            this.streamNameResolver = streamNameResolver;
        }

        public IPersistence<TState> WithSnapshots<TState>(TKey key, Func<TState, Task> applySnapshot)
        {
            return CreatePersistence(key, PersistenceMode.Snapshots, applySnapshot, null);
        }

        public IPersistence<TState> WithSnapshotsAndEventSourcing<TState>(TKey key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return CreatePersistence(key, PersistenceMode.SnapshotsAndEventSourcing, applySnapshot, applyEvent);
        }

        public IPersistence WithEventSourcing(TKey key, Func<Envelope<IEvent>, Task> applyEvent)
        {
            Guard.NotDefault(key, nameof(key));

            var snapshotStore = (ISnapshotStore<object, TKey>)services.GetService(typeof(ISnapshotStore<object, TKey>));

            return new Persistence<TOwner, TKey>(key, invalidate, failed, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, applyEvent);
        }

        private IPersistence<TState> CreatePersistence<TState>(TKey key, PersistenceMode mode, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            Guard.NotDefault(key, nameof(key));

            var snapshotStore = (ISnapshotStore<TState, TKey>)services.GetService(typeof(ISnapshotStore<TState, TKey>));

            return new Persistence<TOwner, TState, TKey>(key, invalidate, failed, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, mode, applySnapshot, applyEvent);
        }
    }
}
