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
        private readonly IServiceProvider services;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;

        public Store(
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IServiceProvider services,
            IStreamNameResolver streamNameResolver,
            Action invalidate = null)
        {
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.invalidate = invalidate;
            this.services = services;
            this.streamNameResolver = streamNameResolver;
        }

        public IPersistence<object> WithEventSourcing<TOwner>(string key, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return CreatePersistence<TOwner, object>(key, null, applyEvent);
        }

        public IPersistence<TState> WithSnapshots<TOwner, TState>(string key, Func<TState, Task> applySnapshot)
        {
            return CreatePersistence<TOwner, TState>(key, applySnapshot, null);
        }

        public IPersistence<TState> WithSnapshotsAndEventSourcing<TOwner, TState>(string key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            return CreatePersistence<TOwner, TState>(key, applySnapshot, applyEvent);
        }

        private IPersistence<TState> CreatePersistence<TOwner, TState>(string key, Func<TState, Task> applySnapshot, Func<Envelope<IEvent>, Task> applyEvent)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            var snapshotStore = (ISnapshotStore<TState>)services.GetService(typeof(ISnapshotStore<TState>));

            return new Persistence<TOwner, TState>(key, invalidate, eventStore, eventDataFormatter, snapshotStore, streamNameResolver, applySnapshot, applyEvent);
        }
    }
}
