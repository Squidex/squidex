// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class PollingSubscription : IEventSubscription
    {
        private readonly RecentEvents recentEvents = new RecentEvents();
        private readonly CompletionTimer timer;

        private sealed class RecentEvents
        {
            private const int Capacity = 50;
            private readonly HashSet<Guid> eventIds = new HashSet<Guid>(Capacity);
            private readonly Queue<(Guid, string)> eventQueue = new Queue<(Guid, string)>(Capacity);

            public string? FirstPosition()
            {
                if (eventQueue.Count == 0)
                {
                    return null;
                }

                return eventQueue.Peek().Item2;
            }

            public bool Add(StoredEvent @event)
            {
                var id = @event.Data.Headers.EventId();

                if (eventIds.Contains(id))
                {
                    return false;
                }

                while (eventQueue.Count >= Capacity)
                {
                    var (storedId, _) = eventQueue.Dequeue();

                    eventIds.Remove(storedId);
                }

                eventIds.Add(id);
                eventQueue.Enqueue((id, @event.EventPosition));

                return true;
            }
        }

        public PollingSubscription(
            IEventStore eventStore,
            IEventSubscriber<StoredEvent> eventSubscriber,
            string? streamFilter,
            string? position)
        {
            timer = new CompletionTimer(5000, async ct =>
            {
                try
                {
                    var newEventCount = 0;
                    do
                    {
                        newEventCount = 0;

                        await foreach (var storedEvent in eventStore.QueryAllAsync(streamFilter, position, ct: ct))
                        {
                            if (recentEvents.Add(storedEvent))
                            {
                                await eventSubscriber.OnNextAsync(this, storedEvent);
                                newEventCount++;
                            }

                            position = recentEvents.FirstPosition();
                        }
                    }
                    while (newEventCount > 50);
                }
                catch (Exception ex)
                {
                    await eventSubscriber.OnErrorAsync(this, ex);
                }
            });
        }

        public ValueTask CompleteAsync()
        {
            return new ValueTask(timer.StopAsync());
        }

        public void Dispose()
        {
            timer.StopAsync().Forget();
        }

        public void WakeUp()
        {
            timer.SkipCurrentDelay();
        }
    }
}
