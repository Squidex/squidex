// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class RetrySubscription : IEventSubscription, IEventSubscriber
    {
        private readonly RetryWindow retryWindow = new RetryWindow(TimeSpan.FromMinutes(5), 5);
        private readonly IEventSubscriber eventSubscriber;
        private readonly Func<IEventSubscriber, IEventSubscription> eventSubscriptionFactory;
        private readonly object lockObject = new object();
        private CancellationTokenSource timerCancellation = new CancellationTokenSource();
        private IEventSubscription? currentSubscription;

        public int ReconnectWaitMs { get; set; } = 5000;

        public object? Sender => currentSubscription?.Sender;

        public RetrySubscription(IEventSubscriber eventSubscriber, Func<IEventSubscriber, IEventSubscription> eventSubscriptionFactory)
        {
            Guard.NotNull(eventSubscriber, nameof(eventSubscriber));
            Guard.NotNull(eventSubscriptionFactory, nameof(eventSubscriptionFactory));

            this.eventSubscriber = eventSubscriber;
            this.eventSubscriptionFactory = eventSubscriptionFactory;

            Subscribe();
        }

        private void Subscribe()
        {
            if (currentSubscription == null)
            {
                lock (lockObject)
                {
                    if (currentSubscription == null)
                    {
                        currentSubscription = eventSubscriptionFactory(this);
                    }
                }
            }
        }

        public void Unsubscribe()
        {
            if (currentSubscription != null)
            {
                lock (lockObject)
                {
                    if (currentSubscription != null)
                    {
                        timerCancellation.Cancel();
                        timerCancellation.Dispose();

                        currentSubscription.Unsubscribe();
                        currentSubscription = null;

                        timerCancellation = new CancellationTokenSource();
                    }
                }
            }
        }

        public void WakeUp()
        {
            currentSubscription?.WakeUp();
        }

        public async Task OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            await eventSubscriber.OnEventAsync(subscription, storedEvent);
        }

        public async Task OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                return;
            }

            if (retryWindow.CanRetryAfterFailure())
            {
                Unsubscribe();

                await Task.Delay(ReconnectWaitMs, timerCancellation.Token);

                Subscribe();
            }
            else
            {
                await eventSubscriber.OnErrorAsync(subscription, exception);
            }
        }
    }
}
