// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Infrastructure.Commands
{
    public abstract class LogSnapshotDomainObject<T> : DomainObjectBase<T> where T : class, IDomainState<T>, new()
    {
        private readonly IStore<DomainId> store;
        private readonly List<T> snapshots = new List<T> { new T { Version = EtagVersion.Empty } };
        private IPersistence? persistence;

        public override T Snapshot
        {
            get { return snapshots.Last(); }
        }

        protected LogSnapshotDomainObject(IStore<DomainId> store, ISemanticLog log)
            : base(log)
        {
            Guard.NotNull(log, nameof(log));

            this.store = store;
        }

        protected override void OnSetup()
        {
            persistence = store.WithEventSourcing(GetType(), UniqueId, x => ApplyEvent(x, true));
        }

        public T GetSnapshot(long version)
        {
            if (version == EtagVersion.Any || version == EtagVersion.Auto)
            {
                return Snapshot;
            }

            if (version == EtagVersion.Empty)
            {
                return snapshots[0];
            }

            if (version >= 0 && version < snapshots.Count - 1)
            {
                return snapshots[(int)version + 1];
            }

            return default!;
        }

        protected sealed override bool ApplyEvent(Envelope<IEvent> @event, bool isLoading)
        {
            var snapshot = OnEvent(@event);

            if (!ReferenceEquals(Snapshot, snapshot) || isLoading)
            {
                var newVersion = Version + 1;

                snapshot.Version = newVersion;
                snapshots.Add(snapshot);

                return true;
            }

            return false;
        }

        protected sealed override async Task WriteAsync(Envelope<IEvent>[] newEvents, long previousVersion)
        {
            if (newEvents.Length > 0 && persistence != null)
            {
                var persistedSnapshots = store.GetSnapshotStore<T>();

                await persistence.WriteEventsAsync(newEvents);
                await persistedSnapshots.WriteAsync(UniqueId, Snapshot, previousVersion, Snapshot.Version);
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
                var persistedSnapshots = store.GetSnapshotStore<T>();

                await persistedSnapshots.WriteAsync(UniqueId, Snapshot, EtagVersion.Any, Snapshot.Version);
            }
        }

        protected sealed override void RestorePreviousSnapshot(T previousSnapshot, long previousVersion)
        {
            while (snapshots.Count > previousVersion + 2)
            {
                snapshots.RemoveAt(snapshots.Count - 1);
            }
        }

        protected T OnEvent(Envelope<IEvent> @event)
        {
            return Snapshot.Apply(@event);
        }
    }
}