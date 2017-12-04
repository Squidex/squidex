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
    public sealed class Store : IStore
    {
        private readonly Action invalidate;
        private readonly ISnapshotStore snapshotStore;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;

        public Store(
            Action invalidate,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore snapshotStore,
            IStreamNameResolver streamNameResolver)
        {
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.invalidate = invalidate;
            this.snapshotStore = snapshotStore;
            this.streamNameResolver = streamNameResolver;
        }

        public IPersistence<object> WithEventSourcing<TOwner>(string key, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return new Persistence<TOwner, object>(key, invalidate, eventStore, eventDataFormatter, null, streamNameResolver, null, applyEvent);
        }

        public IPersistence<TState> WithSnapshots<TOwner, TState>(string key, Func<TState, Task> applySnapshot)
        {
            return new Persistence<TOwner, TState>(key, invalidate, null, null, snapshotStore, null, applySnapshot, null);
        }

        public IPersistence<TState> WithSnapshotsAndEventSourcing<TOwner, TState>(string key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return new Persistence<TOwner, TState>(key, invalidate, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, applySnapshot, applyEvent);
        }
    }
}
