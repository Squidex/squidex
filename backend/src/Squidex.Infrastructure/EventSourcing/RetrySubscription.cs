// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class RetrySubscription<T> : IEventSubscription, IEventSubscriber<T>
    {
        private readonly RetryWindow retryWindow = new RetryWindow(TimeSpan.FromMinutes(5), 5);
        private readonly AsyncLock lockObject = new AsyncLock();
        private readonly IEventSubscriber<T> eventSubscriber;
        private readonly EventSubscriptionSource<T> eventSource;
        private SubscriptionHolder? currentSubscription;

        public int ReconnectWaitMs { get; set; } = 5000;

        // Holds all information for a current subscription. Therefore we only have to maintain one reference.
        private sealed class SubscriptionHolder : IDisposable
        {
            public CancellationTokenSource Cancellation { get; } = new CancellationTokenSource();

            public IEventSubscription Subscription { get; }

            public SubscriptionHolder(IEventSubscription subscription)
            {
                Subscription = subscription;
            }

            public void Dispose()
            {
                Cancellation.Cancel();

                Subscription.Dispose();
            }
        }

        public RetrySubscription(IEventSubscriber<T> eventSubscriber,
            EventSubscriptionSource<T> eventSource)
        {
            Guard.NotNull(eventSubscriber);
            Guard.NotNull(eventSource);

            this.eventSubscriber = eventSubscriber;
            this.eventSource = eventSource;

            Subscribe();
        }

        public void Dispose()
        {
            using (lockObject.Enter())
            {
                Unsubscribe();
            }

            lockObject.Dispose();
        }

        private void Subscribe()
        {
            if (currentSubscription != null)
            {
                return;
            }

            currentSubscription = new SubscriptionHolder(eventSource(this));
        }

        private void Unsubscribe()
        {
            if (currentSubscription == null)
            {
                return;
            }

            currentSubscription.Dispose();
            currentSubscription = null;
        }

        public void WakeUp()
        {
            currentSubscription?.Subscription.WakeUp();
        }

        public ValueTask CompleteAsync()
        {
            return currentSubscription?.Subscription.CompleteAsync() ?? default;
        }

        async ValueTask IEventSubscriber<T>.OnNextAsync(IEventSubscription subscription, T @event)
        {
            // It is not entirely sure, if the lock is needed, but it seems to work so far.
            using (await lockObject.EnterAsync(default))
            {
                if (!ReferenceEquals(subscription, currentSubscription?.Subscription))
                {
                    return;
                }

                await eventSubscriber.OnNextAsync(this, @event);
            }
        }

        async ValueTask IEventSubscriber<T>.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                return;
            }

            using (await lockObject.EnterAsync(default))
            {
                if (!ReferenceEquals(subscription, currentSubscription?.Subscription))
                {
                    return;
                }

                // Unsubscribing is not an atomar operation, therefore the lock.
                Unsubscribe();

                if (!retryWindow.CanRetryAfterFailure())
                {
                    await eventSubscriber.OnErrorAsync(this, exception);
                    return;
                }
            }

            try
            {
                await Task.Delay(ReconnectWaitMs, currentSubscription?.Cancellation?.Token ?? default);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            using (await lockObject.EnterAsync(default))
            {
                // Subscribing is not an atomar operation, therefore the lock.
                Subscribe();
            }
        }
    }
}
