// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObjectGrain<T> : DomainObjectGrainBase<T> where T : class, IDomainState<T>, new()
    {
        private readonly IStore<Guid> store;
        private T snapshot = new T { Version = EtagVersion.Empty };
        private IPersistence<T>? persistence;

        public override T Snapshot
        {
            get { return snapshot; }
        }

        protected DomainObjectGrain(IStore<Guid> store, ISemanticLog log)
            : base(log)
        {
            Guard.NotNull(store);

            this.store = store;
        }

        protected sealed override bool ApplyEvent(Envelope<IEvent> @event, bool isLoading)
        {
            var newVersion = Version + 1;

            var newSnapshot = OnEvent(@event);

            if (!ReferenceEquals(Snapshot, newSnapshot) || isLoading)
            {
                snapshot = newSnapshot;
                snapshot.Version = newVersion;

                return true;
            }

            return false;
        }

        protected sealed override void RestorePreviousSnapshot(T previousSnapshot, long previousVersion)
        {
            snapshot = previousSnapshot;
        }

        protected sealed override Task ReadAsync(Type type, Guid id)
        {
            persistence = store.WithSnapshotsAndEventSourcing(GetType(), id, new HandleSnapshot<T>(ApplySnapshot), x => ApplyEvent(x, true));

            return persistence.ReadAsync();
        }

        private void ApplySnapshot(T state)
        {
            snapshot = state;
        }

        protected sealed override async Task WriteAsync(Envelope<IEvent>[] newEvents, long previousVersion)
        {
            if (newEvents.Length > 0 && persistence != null)
            {
                await persistence.WriteEventsAsync(newEvents);
                await persistence.WriteSnapshotAsync(Snapshot);
            }
        }

        protected T OnEvent(Envelope<IEvent> @event)
        {
            return Snapshot.Apply(@event);
        }
    }
}