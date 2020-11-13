// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Infrastructure.Commands
{
    public abstract class DomainObject<T> : DomainObjectBase<T> where T : class, IDomainState<T>, new()
    {
        private readonly IStore<DomainId> store;
        private T snapshot = new T { Version = EtagVersion.Empty };
        private IPersistence<T>? persistence;

        public override T Snapshot
        {
            get { return snapshot; }
        }

        protected DomainObject(IStore<DomainId> store, ISemanticLog log)
            : base(log)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        protected override void OnSetup()
        {
            persistence = store.WithSnapshotsAndEventSourcing(GetType(), UniqueId, new HandleSnapshot<T>(ApplySnapshot), x => ApplyEvent(x, true));
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

        protected sealed override async Task ReadAsync()
        {
            if (persistence != null)
            {
                await persistence.ReadAsync();
            }
        }

        public sealed override async Task RebuildStateAsync()
        {
            await EnsureLoadedAsync(true);

            if (Snapshot.Version <= EtagVersion.Empty)
            {
                throw new DomainObjectNotFoundException(UniqueId.ToString());
            }

            if (persistence != null)
            {
                await persistence.WriteSnapshotAsync(Snapshot);
            }
        }

        protected T OnEvent(Envelope<IEvent> @event)
        {
            return Snapshot.Apply(@event);
        }
    }
}