﻿// ==========================================================================
//  RetrySubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class RetrySubscription : IEventSubscription, IEventSubscriber
    {
        private readonly SingleThreadedDispatcher dispatcher = new SingleThreadedDispatcher(10);
        private readonly CancellationTokenSource disposeCts = new CancellationTokenSource();
        private readonly RetryWindow retryWindow = new RetryWindow(TimeSpan.FromMinutes(5), 5);
        private readonly IEventStore eventStore;
        private readonly IEventSubscriber eventSubscriber;
        private readonly string streamFilter;
        private IEventSubscription currentSubscription;
        private string position;

        public int ReconnectWaitMs { get; set; } = 5000;

        public RetrySubscription(IEventStore eventStore, IEventSubscriber eventSubscriber, string streamFilter, string position)
        {
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventSubscriber, nameof(eventSubscriber));

            this.position = position;

            this.eventStore = eventStore;
            this.eventSubscriber = eventSubscriber;

            this.streamFilter = streamFilter;

            Subscribe();
        }

        private void Subscribe()
        {
            currentSubscription = eventStore.CreateSubscription(this, streamFilter, position);
        }

        private void Unsubscribe()
        {
            currentSubscription?.StopAsync().Forget();
        }

        private async Task HandleEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            if (subscription == currentSubscription)
            {
                await eventSubscriber.OnEventAsync(this, storedEvent);

                position = storedEvent.EventPosition;
            }
        }

        private async Task HandleErrorAsync(IEventSubscription subscription, Exception exception)
        {
            if (subscription == currentSubscription)
            {
                subscription.StopAsync().Forget();
                subscription = null;

                if (retryWindow.CanRetryAfterFailure())
                {
                    Task.Delay(ReconnectWaitMs, disposeCts.Token).ContinueWith(t =>
                    {
                        dispatcher.DispatchAsync(() => Subscribe());
                    }).Forget();
                }
                else
                {
                    await eventSubscriber.OnErrorAsync(this, exception);
                }
            }
        }

        Task IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            return dispatcher.DispatchAsync(() => HandleEventAsync(subscription, storedEvent));
        }

        Task IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return dispatcher.DispatchAsync(() => HandleErrorAsync(subscription, exception));
        }

        public async Task StopAsync()
        {
            await dispatcher.DispatchAsync(() => Unsubscribe());
            await dispatcher.StopAndWaitAsync();

            disposeCts.Cancel();
        }
    }
}
