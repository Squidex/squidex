// ==========================================================================
//  Store.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.States
{
    public sealed class Store : IStore
    {
        private readonly Action invalidate;
        private readonly ISnapshotStore snapshotStore;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly EventDataFormatter eventDataFormatter;

        public Store(
            ISnapshotStore snapshotStore,
            IStreamNameResolver streamNameResolver,
            IEventStore eventStore,
            EventDataFormatter eventDataFormatter,
            Action invalidate)
        {
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.invalidate = invalidate;
            this.snapshotStore = snapshotStore;
            this.streamNameResolver = streamNameResolver;
        }

        public IPersistence<object> WithEventSourcing<TOwner>(string key, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return new Persistance<TOwner, object>(key, null, streamNameResolver, eventStore, eventDataFormatter, invalidate, null, applyEvent);
        }

        public IPersistence<TState> WithSnapshots<TOwner, TState>(string key, Func<TState, Task> applySnapshot)
        {
            return new Persistance<TOwner, TState>(key, snapshotStore, null, null, null, invalidate, applySnapshot, null);
        }

        public IPersistence<TState> WithSnapshotsAndEventSourcing<TOwner, TState>(string key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return new Persistance<TOwner, TState>(key, snapshotStore, streamNameResolver, eventStore, eventDataFormatter, invalidate, applySnapshot, applyEvent);
        }
    }
}
