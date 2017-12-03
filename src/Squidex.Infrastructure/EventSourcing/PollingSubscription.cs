// ==========================================================================
//  PollingSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class PollingSubscription : IEventSubscription
    {
        private readonly IEventNotifier eventNotifier;
        private readonly IEventStore eventStore;
        private readonly IEventSubscriber eventSubscriber;
        private readonly IDisposable notification;
        private readonly CompletionTimer timer;
        private readonly Regex streamRegex;
        private readonly string streamFilter;
        private string position;

        public PollingSubscription(
            IEventStore eventStore,
            IEventNotifier eventNotifier,
            IEventSubscriber eventSubscriber,
            string streamFilter,
            string position)
        {
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventNotifier, nameof(eventNotifier));
            Guard.NotNull(eventSubscriber, nameof(eventSubscriber));

            this.position = position;
            this.eventNotifier = eventNotifier;
            this.eventStore = eventStore;
            this.eventSubscriber = eventSubscriber;
            this.streamFilter = streamFilter;

            streamRegex = new Regex(streamFilter);

            timer = new CompletionTimer(5000, async ct =>
            {
                try
                {
                    await eventStore.GetEventsAsync(async storedEvent =>
                    {
                        await eventSubscriber.OnEventAsync(this, storedEvent);

                        position = storedEvent.EventPosition;
                    }, ct, streamFilter, position);
                }
                catch (Exception ex)
                {
                    if (!ex.Is<OperationCanceledException>())
                    {
                        await eventSubscriber.OnErrorAsync(this, ex);
                    }
                }
            });

            notification = eventNotifier.Subscribe(streamName =>
            {
                if (streamRegex.IsMatch(streamName))
                {
                    timer.SkipCurrentDelay();
                }
            });
        }

        public Task StopAsync()
        {
            notification?.Dispose();

            return timer.StopAsync();
        }
    }
}
