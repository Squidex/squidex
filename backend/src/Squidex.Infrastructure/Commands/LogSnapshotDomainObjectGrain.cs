// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public abstract class LogSnapshotDomainObjectGrain<T> : DomainObjectGrainBase<T> where T : class, IDomainState<T>, new()
    {
        private readonly IStore<Guid> store;
        private readonly List<T> snapshots = new List<T> { new T { Version = EtagVersion.Empty } };
        private IPersistence? persistence;

        public override T Snapshot
        {
            get { return snapshots.Last(); }
        }

        protected LogSnapshotDomainObjectGrain(IStore<Guid> store, ISemanticLog log)
            : base(log)
        {
            Guard.NotNull(log);

            this.store = store;
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

        protected sealed override Task ReadAsync(Type type, Guid id)
        {
            persistence = store.WithEventSourcing(type, id, x => ApplyEvent(x, true));

            return persistence.ReadAsync();
        }

        protected sealed override async Task WriteAsync(Envelope<IEvent>[] newEvents, long previousVersion)
        {
            if (newEvents.Length > 0 && persistence != null)
            {
                var persistedSnapshots = store.GetSnapshotStore<T>();

                await persistence.WriteEventsAsync(newEvents);
                await persistedSnapshots.WriteAsync(Id, Snapshot, previousVersion, previousVersion + newEvents.Length);
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