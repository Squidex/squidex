// ==========================================================================
//  Persistance.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.States
{
    public sealed class Persistance<TOwner, TState> : IPersistence<TState>
    {
        private readonly string ownerKey;
        private readonly ISnapshotStore snapshotStore;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IEventStore eventStore;
        private readonly EventDataFormatter eventDataFormatter;
        private readonly Action invalidate;
        private readonly Func<TState, Task> applyState;
        private readonly Func<Envelope<IEvent>, Task> applyEvent;
        private Task readTask;
        private int positionSnapshot = -1;
        private int positionEvent = -1;

        public Persistance(string ownerKey,
            ISnapshotStore snapshotStore,
            IStreamNameResolver streamNameResolver,
            IEventStore eventStore,
            EventDataFormatter eventDataFormatter,
            Action invalidate,
            Func<TState, Task> applyState,
            Func<Envelope<IEvent>, Task> applyEvent)
        {
            Guard.NotNull(ownerKey, nameof(ownerKey));

            this.ownerKey = ownerKey;
            this.applyState = applyState;
            this.applyEvent = applyEvent;
            this.invalidate = invalidate;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.snapshotStore = snapshotStore;
            this.streamNameResolver = streamNameResolver;
        }

        public Task ReadAsync(bool force = false)
        {
            if (force)
            {
                return ReadInternalAsync();
            }

            if (readTask == null)
            {
                readTask = ReadInternalAsync();
            }

            return readTask;
        }

        private async Task ReadInternalAsync()
        {
            positionSnapshot = -1;
            positionEvent = -1;

            if (snapshotStore != null)
            {
                var (state, etag) = await snapshotStore.ReadAsync<TState>(ownerKey);

                if (int.TryParse(etag, out var position))
                {
                    positionSnapshot = position;
                    positionEvent = position;

                    if (applyState != null)
                    {
                        await applyState(state);
                    }
                }
            }

            if (eventStore != null && streamNameResolver != null)
            {
                var events = await eventStore.GetEventsAsync(GetStreamName(), positionSnapshot);

                foreach (var @event in events)
                {
                    var parsedEvent = eventDataFormatter.Parse(@event.Data, true);

                    if (applyEvent != null)
                    {
                        await applyEvent(parsedEvent);
                    }

                    positionEvent = (int)@event.EventStreamNumber;
                }
            }
        }

        public async Task WriteSnapShotAsync(TState state)
        {
            if (snapshotStore == null)
            {
                throw new InvalidOperationException("Snapshots are not supported.");
            }

            var newPosition =
                eventStore != null ?
                positionEvent :
                positionSnapshot + 1;

            if (newPosition != positionSnapshot)
            {
                await snapshotStore.WriteAsync(ownerKey, state, positionSnapshot.ToString(), newPosition.ToString());

                positionSnapshot = newPosition;
            }

            invalidate();
        }

        public async Task WriteEventsAsync(params Envelope<IEvent>[] @events)
        {
            Guard.NotNull(events, nameof(@events));

            if (eventStore == null)
            {
                throw new InvalidOperationException("Events are not supported.");
            }

            if (@events.Length > 0)
            {
                var commitId = Guid.NewGuid();

                var eventStream = GetStreamName();
                var eventData = GetEventData(events, commitId);

                await eventStore.AppendEventsAsync(commitId, GetStreamName(), positionEvent, eventData);

                positionEvent += events.Length;
            }

            invalidate();
        }

        private EventData[] GetEventData(Envelope<IEvent>[] events, Guid commitId)
        {
            return @events.Select(x => eventDataFormatter.ToEventData(x, commitId, true)).ToArray();
        }

        private string GetStreamName()
        {
            return streamNameResolver.GetStreamName(typeof(TOwner), ownerKey);
        }
    }
}
