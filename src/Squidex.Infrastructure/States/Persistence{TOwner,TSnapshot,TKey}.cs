// ==========================================================================
//  Persistence.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable RECS0012 // 'if' statement can be re-written as 'switch' statement

namespace Squidex.Infrastructure.States
{
    internal class Persistence<TOwner, TSnapshot, TKey> : IPersistence<TSnapshot>
    {
        private readonly TKey ownerKey;
        private readonly ISnapshotStore<TSnapshot, TKey> snapshotStore;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly PersistenceMode persistenceMode;
        private readonly Action invalidate;
        private readonly Action failed;
        private readonly Func<TSnapshot, Task> applyState;
        private readonly Func<Envelope<IEvent>, Task> applyEvent;
        private long versionSnapshot = EtagVersion.Empty;
        private long versionEvents = EtagVersion.Empty;
        private long version;

        public long Version
        {
            get { return version; }
        }

        public Persistence(TKey ownerKey,
            Action invalidate,
            Action failed,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<TSnapshot, TKey> snapshotStore,
            IStreamNameResolver streamNameResolver,
            PersistenceMode persistenceMode,
            Func<TSnapshot, Task> applyState,
            Func<Envelope<IEvent>, Task> applyEvent)
        {
            this.ownerKey = ownerKey;
            this.applyState = applyState;
            this.applyEvent = applyEvent;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.invalidate = invalidate;
            this.failed = failed;
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
                    throw new DomainObjectNotFoundException(ownerKey.ToString(), typeof(TOwner));
                }
                else
                {
                    throw new DomainObjectVersionException(ownerKey.ToString(), typeof(TOwner), version, expectedVersion);
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
                    await applyState(state);
                }
            }
        }

        private async Task ReadEventsAsync()
        {
            if (UseEventSourcing())
            {
                var events = await eventStore.GetEventsAsync(GetStreamName(), versionEvents + 1);

                foreach (var @event in events)
                {
                    versionEvents++;

                    if (@event.EventStreamNumber != versionEvents)
                    {
                        throw new InvalidOperationException("Events must follow the snapshot version in consecutive order with no gaps.");
                    }

                    var parsedEvent = ParseKnownEvent(@event);

                    if (parsedEvent != null && applyEvent != null)
                    {
                        await applyEvent(parsedEvent);
                    }
                }
            }
        }

        public async Task WriteSnapshotAsync(TSnapshot state)
        {
            try
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
                        throw new DomainObjectVersionException(ownerKey.ToString(), typeof(TOwner), ex.CurrentVersion, ex.ExpectedVersion);
                    }

                    versionSnapshot = newVersion;
                }

                UpdateVersion();

                invalidate?.Invoke();
            }
            catch
            {
                failed?.Invoke();

                throw;
            }
        }

        public async Task WriteEventsAsync(IEnumerable<Envelope<IEvent>> events)
        {
            Guard.NotNull(events, nameof(@events));

            try
            {
                var eventArray = events.ToArray();

                if (eventArray.Length > 0)
                {
                    var expectedVersion = UseEventSourcing() ? version : EtagVersion.Any;

                    var commitId = Guid.NewGuid();

                    var eventStream = GetStreamName();
                    var eventData = GetEventData(eventArray, commitId);

                    try
                    {
                        await eventStore.AppendEventsAsync(commitId, GetStreamName(), expectedVersion, eventData);
                    }
                    catch (WrongEventVersionException ex)
                    {
                        throw new DomainObjectVersionException(ownerKey.ToString(), typeof(TOwner), ex.CurrentVersion, ex.ExpectedVersion);
                    }

                    versionEvents += eventArray.Length;
                }

                UpdateVersion();

                invalidate?.Invoke();
            }
            catch
            {
                failed?.Invoke();

                throw;
            }
        }

        private EventData[] GetEventData(Envelope<IEvent>[] events, Guid commitId)
        {
            return @events.Select(x => eventDataFormatter.ToEventData(x, commitId, true)).ToArray();
        }

        private string GetStreamName()
        {
            return streamNameResolver.GetStreamName(typeof(TOwner), ownerKey.ToString());
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
