// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectBase<T> : IDomainObject where T : IDomainState, new()
    {
        private readonly List<Envelope<IEvent>> uncomittedEvents = new List<Envelope<IEvent>>();
        private Guid id;
        private T snapshot = new T { Version = EtagVersion.Empty };
        private IPersistence<T> persistence;

        public long Version
        {
            get { return snapshot.Version; }
        }

        public T Snapshot
        {
            get { return snapshot; }
        }

        public Task ActivateAsync(Guid key, IStore<Guid> store)
        {
            id = key;

            persistence = store.WithSnapshotsAndEventSourcing<T, Guid>(key, ApplySnapshot, ApplyEvent);

            return persistence.ReadAsync();
        }

        public void RaiseEvent(IEvent @event)
        {
            RaiseEvent(Envelope.Create(@event));
        }

        public void RaiseEvent<TEvent>(Envelope<TEvent> @event) where TEvent : class, IEvent
        {
            Guard.NotNull(@event, nameof(@event));

            @event.SetAggregateId(id);

            ApplyEvent(@event.To<IEvent>());

            snapshot.Version++;

            uncomittedEvents.Add(@event.To<IEvent>());
        }

        public IReadOnlyList<Envelope<IEvent>> GetUncomittedEvents()
        {
            return uncomittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            uncomittedEvents.Clear();
        }

        public virtual void ApplySnapshot(T newSnapshot)
        {
            snapshot = newSnapshot;
        }

        public virtual void ApplyEvent(Envelope<IEvent> @event)
        {
        }

        public Task WriteSnapshotAsync()
        {
            if (persistence.Version == EtagVersion.NotFound)
            {
                Debugger.Break();
            }

            snapshot.Version = persistence.Version;

            return persistence.WriteSnapshotAsync(snapshot);
        }

        public async Task WriteAsync()
        {
            var events = uncomittedEvents.ToArray();

            if (events.Length > 0)
            {
                try
                {
                    snapshot.Version = persistence.Version + events.Length;

                    await persistence.WriteEventsAsync(events);
                    await persistence.WriteSnapshotAsync(snapshot);
                }
                finally
                {
                    uncomittedEvents.Clear();
                }
            }
        }
    }
}
