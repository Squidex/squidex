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
        private const string ContextKey = "RecentEvents";
        private readonly CompletionTimer timer;

        public PollingSubscription(
            IEventStore eventStore,
            IEventSubscriber<StoredEvent> eventSubscriber,
            SubscriptionQuery query,
            bool queryOnce = false)
        {
            // Depending on the implementation of the event store it is not guaranteed that no event is added
            // with an older position than what we are currently queried.
            // Therefore we use overlapping query windows of 50 events (this is just a guess).
            var recentEvents = RecentEvents.Parse(query.Context?.GetOrDefault(ContextKey));

            var position = recentEvents.FirstPosition() ?? query.Position;

            timer = new CompletionTimer(5000, async ct =>
            {
                try
                {
                    // If we have read events it is very likely that there will be more coming and we just query again immediately.
                    var newEventCount = 0;
                    do
                    {
                        newEventCount = 0;

                        await foreach (var @event in eventStore.QueryAllAsync(query.StreamFilter, position, ct: ct))
                        {
                            var storedEvent = @event;

                            // Check if we have received the event already in the latest query.
                            if (recentEvents.Add(storedEvent))
                            {
                                var recentEventString = recentEvents.ToString();

                                if (recentEventString != null)
                                {
                                    // Serialize the recent events to have them available with the next run.
                                    storedEvent = storedEvent with
                                    {
                                        Context = new Dictionary<string, string>
                                        {
                                            [ContextKey] = recentEventString
                                        }
                                    };
                                }

                                await eventSubscriber.OnNextAsync(this, storedEvent);
                                newEventCount++;
                            }
                        }

                        // Use the first position from the window.
                        position = recentEvents.FirstPosition();
                    }
                    // Use a value greater than one, because otherwise we would always query 51 events (because of the overlapping window) just for another event.
                    while (newEventCount > 50 && !queryOnce);
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
