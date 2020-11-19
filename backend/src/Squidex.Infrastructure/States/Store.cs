// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public sealed class Store<TKey> : IStore<TKey> where TKey : notnull
    {
        private readonly IServiceProvider services;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventEnricher<TKey> eventEnricher;
        private readonly IEventDataFormatter eventDataFormatter;

        public Store(
            IEventStore eventStore,
            IEventEnricher<TKey> eventEnricher,
            IEventDataFormatter eventDataFormatter,
            IServiceProvider services,
            IStreamNameResolver streamNameResolver)
        {
            this.eventStore = eventStore;
            this.eventEnricher = eventEnricher;
            this.eventDataFormatter = eventDataFormatter;
            this.services = services;
            this.streamNameResolver = streamNameResolver;
        }

        public IPersistence WithEventSourcing(Type owner, TKey key, HandleEvent? applyEvent)
        {
            return CreatePersistence(owner, key, applyEvent);
        }

        public IPersistence<TState> WithSnapshots<TState>(Type owner, TKey key, HandleSnapshot<TState>? applySnapshot)
        {
            return CreatePersistence(owner, key, PersistenceMode.Snapshots, applySnapshot, null);
        }

        public IPersistence<TState> WithSnapshotsAndEventSourcing<TState>(Type owner, TKey key, HandleSnapshot<TState>? applySnapshot, HandleEvent? applyEvent)
        {
            return CreatePersistence(owner, key, PersistenceMode.SnapshotsAndEventSourcing, applySnapshot, applyEvent);
        }

        private IPersistence CreatePersistence(Type owner, TKey key, HandleEvent? applyEvent)
        {
            Guard.NotNull(key, nameof(key));

            var snapshotStore = GetSnapshotStore<None>();

            return new Persistence<TKey>(key, owner, eventStore, eventEnricher, eventDataFormatter, snapshotStore, streamNameResolver, applyEvent);
        }

        private IPersistence<TState> CreatePersistence<TState>(Type owner, TKey key, PersistenceMode mode, HandleSnapshot<TState>? applySnapshot, HandleEvent? applyEvent)
        {
            Guard.NotNull(key, nameof(key));

            var snapshotStore = GetSnapshotStore<TState>();

            return new Persistence<TState, TKey>(key, owner, eventStore, eventEnricher, eventDataFormatter, snapshotStore, streamNameResolver, mode, applySnapshot, applyEvent);
        }

        public ISnapshotStore<TState, TKey> GetSnapshotStore<TState>()
        {
            return (ISnapshotStore<TState, TKey>)services.GetService(typeof(ISnapshotStore<TState, TKey>))!;
        }
    }
}
