// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public sealed class Store<T> : IStore<T>
    {
        private readonly IStreamNameResolver streamNameResolver;
        private readonly ISnapshotStore<T> snapshotStore;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;

        public Store(
            ISnapshotStore<T> snapshotStore,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IStreamNameResolver streamNameResolver)
        {
            this.snapshotStore = snapshotStore;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.streamNameResolver = streamNameResolver;
        }

        public Task ClearSnapshotsAsync()
        {
            return snapshotStore.ClearAsync();
        }

        public IBatchContext<T> WithBatchContext(Type owner)
        {
            return new BatchContext<T>(owner,
                snapshotStore,
                eventStore,
                eventDataFormatter,
                streamNameResolver);
        }

        public IPersistence<T> WithEventSourcing(Type owner, DomainId key, HandleEvent? applyEvent)
        {
            return CreatePersistence(owner, key, PersistenceMode.EventSourcing, null, applyEvent);
        }

        public IPersistence<T> WithSnapshots(Type owner, DomainId key, HandleSnapshot<T>? applySnapshot)
        {
            return CreatePersistence(owner, key, PersistenceMode.Snapshots, applySnapshot, null);
        }

        public IPersistence<T> WithSnapshotsAndEventSourcing(Type owner, DomainId key, HandleSnapshot<T>? applySnapshot, HandleEvent? applyEvent)
        {
            return CreatePersistence(owner, key, PersistenceMode.SnapshotsAndEventSourcing, applySnapshot, applyEvent);
        }

        private IPersistence<T> CreatePersistence(Type owner, DomainId key, PersistenceMode mode, HandleSnapshot<T>? applySnapshot, HandleEvent? applyEvent)
        {
            Guard.NotNull(key, nameof(key));

            return new Persistence<T>(key, owner,
                snapshotStore,
                eventStore,
                eventDataFormatter,
                streamNameResolver,
                mode,
                applySnapshot,
                applyEvent);
        }
    }
}
