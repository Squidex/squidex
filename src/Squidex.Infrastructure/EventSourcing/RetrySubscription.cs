// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class RetrySubscription : IEventSubscription, IEventSubscriber
    {
        private readonly SingleThreadedDispatcher dispatcher = new SingleThreadedDispatcher(10);
        private readonly CancellationTokenSource timerCts = new CancellationTokenSource();
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
            Guard.NotNull(streamFilter, nameof(streamFilter));

            this.position = position;

            this.eventStore = eventStore;
            this.eventSubscriber = eventSubscriber;

            this.streamFilter = streamFilter;

            Subscribe();
        }

        private void Subscribe()
        {
            if (currentSubscription == null)
            {
                currentSubscription = eventStore.CreateSubscription(this, streamFilter, position);
            }
        }

        private void Unsubscribe()
        {
            currentSubscription?.StopAsync().Forget();
            currentSubscription = null;
        }

        public void WakeUp()
        {
            currentSubscription?.WakeUp();
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
                Unsubscribe();

                if (retryWindow.CanRetryAfterFailure())
                {
                    Task.Delay(ReconnectWaitMs, timerCts.Token).ContinueWith(t =>
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

            timerCts.Cancel();
        }
    }
}
