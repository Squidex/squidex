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
    public abstract class LogSnapshotDomainObjectGrain<T> : DomainObjectGrainBase<T> where T : IDomainState, new()
    {
        private readonly IStore<Guid> store;
        private readonly List<T> snapshots = new List<T> { new T { Version = EtagVersion.Empty } };
        private IPersistence persistence;

        public override T Snapshot
        {
            get { return snapshots.Last(); }
        }

        protected LogSnapshotDomainObjectGrain(IStore<Guid> store, ISemanticLog log)
            : base(log)
        {
            Guard.NotNull(log, nameof(log));

            this.store = store;
        }

        public T GetSnapshot(long version)
        {
            if (version == EtagVersion.Any)
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

            return default(T);
        }

        protected sealed override void ApplyEvent(Envelope<IEvent> @event)
        {
            var snapshot = OnEvent(@event);

            snapshot.Version = Version + 1;
            snapshots.Add(snapshot);
        }

        protected sealed override Task ReadAsync(Type type, Guid id)
        {
            persistence = store.WithEventSourcing<Guid>(type, id, ApplyEvent);

            return persistence.ReadAsync();
        }

        protected sealed override async Task WriteAsync(Envelope<IEvent>[] events, long previousVersion)
        {
            if (events.Length > 0)
            {
                var persistedSnapshots = store.GetSnapshotStore<T>();

                await persistence.WriteEventsAsync(events);
                await persistedSnapshots.WriteAsync(Id, Snapshot, previousVersion, previousVersion + events.Length);
            }
        }

        protected sealed override void RestorePreviousSnapshot(T previousSnapshot, long previousVersion)
        {
            while (snapshots.Count > previousVersion + 2)
            {
                snapshots.RemoveAt(snapshots.Count - 1);
            }
        }

        protected abstract T OnEvent(Envelope<IEvent> @event);
    }
}