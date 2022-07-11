// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class RetrySubscription : IEventSubscription, IEventSubscriber
    {
        private readonly RetryWindow retryWindow = new RetryWindow(TimeSpan.FromMinutes(5), 5);
        private readonly AsyncLock lockObject = new AsyncLock();
        private readonly IEventSubscriber eventSubscriber;
        private readonly Func<IEventSubscriber, IEventSubscription> eventSubscriptionFactory;
        private CancellationTokenSource timerCancellation = new CancellationTokenSource();
        private IEventSubscription? currentSubscription;

        public int ReconnectWaitMs { get; set; } = 5000;

        public RetrySubscription(IEventSubscriber eventSubscriber, Func<IEventSubscriber, IEventSubscription> eventSubscriptionFactory)
        {
            Guard.NotNull(eventSubscriber);
            Guard.NotNull(eventSubscriptionFactory);

            this.eventSubscriber = eventSubscriber;
            this.eventSubscriptionFactory = eventSubscriptionFactory;

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

            currentSubscription = eventSubscriptionFactory(this);
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

        async ValueTask IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            using (await lockObject.EnterAsync(default))
            {
                if (!ReferenceEquals(subscription, currentSubscription))
                {
                    return;
                }

                await eventSubscriber.OnEventAsync(this, storedEvent);
            }
        }

        async ValueTask IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
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
