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
    internal class Persistence<TSnapshot, TKey> : IPersistence<TSnapshot> where TKey : notnull
    {
        private readonly TKey ownerKey;
        private readonly ISnapshotStore<TSnapshot, TKey> snapshotStore;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly PersistenceMode persistenceMode;
        private readonly HandleSnapshot<TSnapshot>? applyState;
        private readonly HandleEvent? applyEvent;
        private readonly Lazy<string> streamName;
        private long versionSnapshot = EtagVersion.Empty;
        private long versionEvents = EtagVersion.Empty;
        private long version = EtagVersion.Empty;

        public long Version
        {
            get => version;
        }

        private bool UseSnapshots
        {
            get => (persistenceMode & PersistenceMode.Snapshots) == PersistenceMode.Snapshots;
        }

        private bool UseEventSourcing
        {
            get => (persistenceMode & PersistenceMode.EventSourcing) == PersistenceMode.EventSourcing;
        }

        public Persistence(TKey ownerKey, Type ownerType,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<TSnapshot, TKey> snapshotStore,
            IStreamNameResolver streamNameResolver,
            PersistenceMode persistenceMode,
            HandleSnapshot<TSnapshot>? applyState,
            HandleEvent? applyEvent)
        {
            this.ownerKey = ownerKey;
            this.applyState = applyState;
            this.applyEvent = applyEvent;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.persistenceMode = persistenceMode;
            this.snapshotStore = snapshotStore;

            streamName = new Lazy<string>(() => streamNameResolver.GetStreamName(ownerType, ownerKey.ToString()!));
        }

        public async Task DeleteAsync()
        {
            if (UseSnapshots)
            {
                await snapshotStore.RemoveAsync(ownerKey);
            }

            if (UseEventSourcing)
            {
                await eventStore.DeleteStreamAsync(streamName.Value);
            }
        }

        public async Task ReadAsync(long expectedVersion = EtagVersion.Any)
        {
            versionSnapshot = EtagVersion.Empty;
            versionEvents = EtagVersion.Empty;

            if (UseSnapshots)
            {
                await ReadSnapshotAsync();
            }

            if (UseEventSourcing)
            {
                await ReadEventsAsync();
            }

            UpdateVersion();

            if (expectedVersion > EtagVersion.Any && expectedVersion != version)
            {
                if (version == EtagVersion.Empty)
                {
                    throw new DomainObjectNotFoundException(ownerKey.ToString()!);
                }
                else
                {
                    throw new InconsistentStateException(version, expectedVersion);
                }
            }
        }

        private async Task ReadSnapshotAsync()
        {
            var (state, position) = await snapshotStore.ReadAsync(ownerKey);

            // Treat all negative values as not-found (empty).
            position = Math.Max(position, EtagVersion.Empty);

            versionSnapshot = position;
            versionEvents = position;

            if (applyState != null && position >= 0)
            {
                applyState(state);
            }
        }

        private async Task ReadEventsAsync()
        {
            var events = await eventStore.QueryAsync(streamName.Value, versionEvents + 1);

            var isStopped = false;

            foreach (var @event in events)
            {
                var newVersion = versionEvents + 1;

                if (@event.EventStreamNumber != newVersion)
                {
                    throw new InvalidOperationException("Events must follow the snapshot version in consecutive order with no gaps.");
                }

                // Skip the parsing for performance reasons if we are not interested, but continue reading to get the version.
                if (!isStopped)
                {
                    var parsedEvent = eventDataFormatter.ParseIfKnown(@event);

                    if (applyEvent != null && parsedEvent != null)
                    {
                        isStopped = !applyEvent(parsedEvent);
                    }
                }

                versionEvents++;
            }
        }

        public async Task WriteSnapshotAsync(TSnapshot state)
        {
            var oldVersion = versionSnapshot;

            if (oldVersion == EtagVersion.Empty && UseEventSourcing)
            {
                oldVersion = (versionEvents - 1);
            }

            var newVersion = UseEventSourcing ? versionEvents : oldVersion + 1;

            if (newVersion == versionSnapshot)
            {
                return;
            }

            await snapshotStore.WriteAsync(ownerKey, state, oldVersion, newVersion);

            versionSnapshot = newVersion;

            UpdateVersion();
        }

        public async Task WriteEventsAsync(IReadOnlyList<Envelope<IEvent>> events)
        {
            Guard.NotEmpty(events, nameof(events));

            var oldVersion = EtagVersion.Any;

            if (UseEventSourcing)
            {
                oldVersion = versionEvents;
            }

            var eventCommitId = Guid.NewGuid();
            var eventData = events.Select(x => eventDataFormatter.ToEventData(x, eventCommitId, true)).ToArray();

            try
            {
                await eventStore.AppendAsync(eventCommitId, streamName.Value, oldVersion, eventData);
            }
            catch (WrongEventVersionException ex)
            {
                throw new InconsistentStateException(ex.CurrentVersion, ex.ExpectedVersion, ex);
            }

            versionEvents += eventData.Length;
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
