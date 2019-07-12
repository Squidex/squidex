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

#pragma warning disable RECS0012 // 'if' statement can be re-written as 'switch' statement

namespace Squidex.Infrastructure.States
{
    internal class Persistence<TSnapshot, TKey> : IPersistence<TSnapshot>
    {
        private readonly TKey ownerKey;
        private readonly Type ownerType;
        private readonly ISnapshotStore<TSnapshot, TKey> snapshotStore;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly PersistenceMode persistenceMode;
        private readonly HandleSnapshot<TSnapshot> applyState;
        private readonly HandleEvent applyEvent;
        private long versionSnapshot = EtagVersion.Empty;
        private long versionEvents = EtagVersion.Empty;
        private long version;

        public long Version
        {
            get { return version; }
        }

        public Persistence(TKey ownerKey, Type ownerType,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<TSnapshot, TKey> snapshotStore,
            IStreamNameResolver streamNameResolver,
            PersistenceMode persistenceMode,
            HandleSnapshot<TSnapshot> applyState,
            HandleEvent applyEvent)
        {
            this.ownerKey = ownerKey;
            this.ownerType = ownerType;
            this.applyState = applyState;
            this.applyEvent = applyEvent;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.persistenceMode = persistenceMode;
            this.snapshotStore = snapshotStore;
            this.streamNameResolver = streamNameResolver;
        }

        public async Task ReadAsync(long expectedVersion = EtagVersion.Any)
        {
            versionSnapshot = EtagVersion.Empty;
            versionEvents = EtagVersion.Empty;

            await ReadSnapshotAsync();
            await ReadEventsAsync();

            UpdateVersion();

            if (expectedVersion != EtagVersion.Any && expectedVersion != version)
            {
                if (version == EtagVersion.Empty)
                {
                    throw new DomainObjectNotFoundException(ownerKey.ToString(), ownerType);
                }
                else
                {
                    throw new DomainObjectVersionException(ownerKey.ToString(), ownerType, version, expectedVersion);
                }
            }
        }

        private async Task ReadSnapshotAsync()
        {
            if (UseSnapshots())
            {
                var (state, position) = await snapshotStore.ReadAsync(ownerKey);

                if (position < EtagVersion.Empty)
                {
                    position = EtagVersion.Empty;
                }

                versionSnapshot = position;
                versionEvents = position;

                if (applyState != null && position >= 0)
                {
                    applyState(state);
                }
            }
        }

        private async Task ReadEventsAsync()
        {
            if (UseEventSourcing())
            {
                var events = await eventStore.QueryAsync(GetStreamName(), versionEvents + 1);

                foreach (var @event in events)
                {
                    versionEvents++;

                    if (@event.EventStreamNumber != versionEvents)
                    {
                        throw new InvalidOperationException("Events must follow the snapshot version in consecutive order with no gaps.");
                    }

                    var parsedEvent = ParseKnownEvent(@event);

                    if (applyEvent != null && parsedEvent != null)
                    {
                        applyEvent(parsedEvent);
                    }
                }
            }
        }

        public async Task WriteSnapshotAsync(TSnapshot state)
        {
            var newVersion = UseEventSourcing() ? versionEvents : versionSnapshot + 1;

            if (newVersion != versionSnapshot)
            {
                try
                {
                    await snapshotStore.WriteAsync(ownerKey, state, versionSnapshot, newVersion);
                }
                catch (InconsistentStateException ex)
                {
                    throw new DomainObjectVersionException(ownerKey.ToString(), ownerType, ex.CurrentVersion, ex.ExpectedVersion);
                }

                versionSnapshot = newVersion;
            }

            UpdateVersion();
        }

        public async Task WriteEventsAsync(IEnumerable<Envelope<IEvent>> events)
        {
            Guard.NotNull(events, nameof(events));

            var eventArray = events.ToArray();

            if (eventArray.Length > 0)
            {
                var expectedVersion = UseEventSourcing() ? version : EtagVersion.Any;

                var commitId = Guid.NewGuid();

                var eventStream = GetStreamName();
                var eventData = GetEventData(eventArray, commitId);

                try
                {
                    await eventStore.AppendAsync(commitId, eventStream, expectedVersion, eventData);
                }
                catch (WrongEventVersionException ex)
                {
                    throw new DomainObjectVersionException(ownerKey.ToString(), ownerType, ex.CurrentVersion, ex.ExpectedVersion);
                }

                versionEvents += eventArray.Length;
            }

            UpdateVersion();
        }

        public async Task DeleteAsync()
        {
            if (UseEventSourcing())
            {
                await eventStore.DeleteStreamAsync(GetStreamName());
            }

            if (UseSnapshots())
            {
                await snapshotStore.RemoveAsync(ownerKey);
            }
        }

        private EventData[] GetEventData(Envelope<IEvent>[] events, Guid commitId)
        {
            return events.Map(x => eventDataFormatter.ToEventData(x, commitId, true));
        }

        private string GetStreamName()
        {
            return streamNameResolver.GetStreamName(ownerType, ownerKey.ToString());
        }

        private bool UseSnapshots()
        {
            return persistenceMode == PersistenceMode.Snapshots || persistenceMode == PersistenceMode.SnapshotsAndEventSourcing;
        }

        private bool UseEventSourcing()
        {
            return persistenceMode == PersistenceMode.EventSourcing || persistenceMode == PersistenceMode.SnapshotsAndEventSourcing;
        }

        private Envelope<IEvent> ParseKnownEvent(StoredEvent storedEvent)
        {
            try
            {
                return eventDataFormatter.Parse(storedEvent.Data);
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
        }

        private void UpdateVersion()
        {
            if (persistenceMode == PersistenceMode.Snapshots)
            {
                version = versionSnapshot;
            }
            else if (persistenceMode == PersistenceMode.EventSourcing)
            {
                version = versionEvents;
            }
            else if (persistenceMode == PersistenceMode.SnapshotsAndEventSourcing)
            {
                version = Math.Max(versionEvents, versionSnapshot);
            }
        }
    }
}
