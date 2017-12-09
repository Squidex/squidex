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

namespace Squidex.Infrastructure.States
{
    internal sealed class Persistence<TOwner, TState> : IPersistence<TState>
    {
        private readonly string ownerKey;
        private readonly ISnapshotStore<TState> snapshotStore;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly Action invalidate;
        private readonly Func<TState, Task> applyState;
        private readonly Func<Envelope<IEvent>, Task> applyEvent;
        private long positionSnapshot = -1;
        private long positionEvent = -1;

        public long Version
        {
            get { return Math.Max(positionEvent, positionSnapshot); }
        }

        public Persistence(string ownerKey,
            Action invalidate,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISnapshotStore<TState> snapshotStore,
            IStreamNameResolver streamNameResolver,
            Func<TState, Task> applyState,
            Func<Envelope<IEvent>, Task> applyEvent)
        {
            this.ownerKey = ownerKey;
            this.applyState = applyState;
            this.applyEvent = applyEvent;
            this.invalidate = invalidate;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.snapshotStore = snapshotStore;
            this.streamNameResolver = streamNameResolver;
        }

        public async Task ReadAsync(long? expectedVersion)
        {
            positionSnapshot = -1;
            positionEvent = -1;

            if (applyState != null)
            {
                var (state, position) = await snapshotStore.ReadAsync(ownerKey);

                positionSnapshot = position;
                positionEvent = position;

                if (applyState != null && position >= 0)
                {
                    await applyState(state);
                }
            }

            if (applyEvent != null && streamNameResolver != null)
            {
                var events = await eventStore.GetEventsAsync(GetStreamName(), positionEvent + 1);

                foreach (var @event in events)
                {
                    positionEvent++;

                    if (@event.EventStreamNumber != positionEvent)
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

            var newVersion = Version;

            if (expectedVersion.HasValue && expectedVersion.Value != newVersion)
            {
                if (newVersion == -1)
                {
                    throw new DomainObjectNotFoundException(ownerKey, typeof(TOwner));
                }
                else
                {
                    throw new DomainObjectVersionException(ownerKey, typeof(TOwner), newVersion, expectedVersion.Value);
                }
            }
        }

        public async Task WriteSnapshotAsync(TState state, long newVersion = -1)
        {
            if (newVersion < 0)
            {
                newVersion =
                    applyEvent != null ?
                    positionEvent :
                    positionSnapshot + 1;
            }

            if (newVersion != positionSnapshot)
            {
                try
                {
                    await snapshotStore.WriteAsync(ownerKey, state, positionSnapshot, newVersion);
                }
                catch (InconsistentStateException ex)
                {
                    throw new DomainObjectVersionException(ownerKey, typeof(TOwner), ex.CurrentVersion, ex.ExpectedVersion);
                }

                positionSnapshot = newVersion;
            }

            invalidate?.Invoke();
        }

        public async Task WriteEventsAsync(IEnumerable<Envelope<IEvent>> events)
        {
            Guard.NotNull(events, nameof(@events));

            var eventArray = events.ToArray();

            if (eventArray.Length > 0)
            {
                var commitId = Guid.NewGuid();

                var eventStream = GetStreamName();
                var eventData = GetEventData(eventArray, commitId);

                try
                {
                    await eventStore.AppendEventsAsync(commitId, GetStreamName(), Version, eventData);
                }
                catch (WrongEventVersionException ex)
                {
                    throw new DomainObjectVersionException(ownerKey, typeof(TOwner), ex.CurrentVersion, ex.ExpectedVersion);
                }

                positionEvent += eventArray.Length;
            }

            invalidate?.Invoke();
        }

        private EventData[] GetEventData(Envelope<IEvent>[] events, Guid commitId)
        {
            return @events.Select(x => eventDataFormatter.ToEventData(x, commitId, true)).ToArray();
        }

        private string GetStreamName()
        {
            return streamNameResolver.GetStreamName(typeof(TOwner), ownerKey);
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
    }
}
