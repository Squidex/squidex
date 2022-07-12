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
        private CancellationTokenSource timerCancellation = new CancellationTokenSource();
        private IEventSubscription? currentSubscription;

        public int ReconnectWaitMs { get; set; } = 5000;

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

            currentSubscription = eventSource(this);
        }

        private void Unsubscribe()
        {
            if (currentSubscription == null)
            {
                return;
            }

            timerCancellation.Cancel();
            timerCancellation.Dispose();

            currentSubscription.Dispose();
            currentSubscription = null;

            timerCancellation = new CancellationTokenSource();
        }

        public void WakeUp()
        {
            currentSubscription?.WakeUp();
        }

        public ValueTask CompleteAsync()
        {
            return currentSubscription?.CompleteAsync() ?? default;
        }

        async ValueTask IEventSubscriber<T>.OnNextAsync(IEventSubscription subscription, T @event)
        {
            using (await lockObject.EnterAsync(default))
            {
                if (!ReferenceEquals(subscription, currentSubscription))
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
                if (!ReferenceEquals(subscription, currentSubscription))
                {
                    return;
                }

                Unsubscribe();

                if (!retryWindow.CanRetryAfterFailure())
                {
                    await eventSubscriber.OnErrorAsync(this, exception);
                    return;
                }
            }

            try
            {
                await Task.Delay(ReconnectWaitMs, timerCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            using (await lockObject.EnterAsync(default))
            {
                Subscribe();
            }
        }
    }
}
