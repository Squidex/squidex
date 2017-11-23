// ==========================================================================
//  EventStoreSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventStoreSubscription : IEventSubscription
    {
        private readonly IEventStore eventStore;
        private readonly IEventSubscriber eventSubscriber;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly Task task;
        private readonly string streamFilter;

        public EventStoreSubscription(
            IEventStore eventStore,
            IEventSubscriber eventSubscriber,
            string streamFilter,
            string position)
        {
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventSubscriber, nameof(eventSubscriber));

            this.eventStore = eventStore;
            this.eventSubscriber = eventSubscriber;
            this.streamFilter = streamFilter;

            task = Task.Run(async () =>
            {
                try
                {
                    await eventStore.GetEventsAsync(async storedEvent =>
                    {
                        await eventSubscriber.OnEventAsync(this, storedEvent);

                        position = storedEvent.EventPosition;
                    }, cts.Token, streamFilter, position);
                }
                catch (Exception ex)
                {
                    if (!ex.Is<OperationCanceledException>())
                    {
                        await eventSubscriber.OnErrorAsync(this, ex);
                    }
                }
            });
        }

        public Task StopAsync()
        {
            cts.Cancel();

            return task;
        }
    }
}
