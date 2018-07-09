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
    public abstract class DomainObjectGrain<T> : DomainObjectGrainBase<T> where T : IDomainState, new()
    {
        private readonly IStore<Guid> store;
        private T snapshot = new T { Version = EtagVersion.Empty };
        private IPersistence<T> persistence;

        public override T Snapshot
        {
            get { return snapshot; }
        }

        protected DomainObjectGrain(IStore<Guid> store, ISemanticLog log)
            : base(log)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        protected sealed override void ApplyEvent(Envelope<IEvent> @event)
        {
            var newVersion = Version + 1;

            var snapshotNew = OnEvent(@event);

            snapshot = OnEvent(@event);
            snapshot.Version = newVersion;
        }

        protected sealed override void RestorePreviousSnapshot(T previousSnapshot, long previousVersion)
        {
            snapshot = previousSnapshot;
        }

        protected sealed override Task ReadAsync(Type type, Guid id)
        {
            persistence = store.WithSnapshotsAndEventSourcing<T, Guid>(GetType(), id, x => snapshot = x, ApplyEvent);

            return persistence.ReadAsync();
        }

        protected sealed override async Task WriteAsync(Envelope<IEvent>[] events, long previousVersion)
        {
            if (events.Length > 0)
            {
                await persistence.WriteEventsAsync(events);
                await persistence.WriteSnapshotAsync(Snapshot);
            }
        }

        protected abstract T OnEvent(Envelope<IEvent> @event);
    }
}