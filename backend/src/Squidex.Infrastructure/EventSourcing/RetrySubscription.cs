// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public sealed class RetrySubscription<T> : IEventSubscription, IEventSubscriber<T>
{
    private readonly RetryWindow retryWindow = new RetryWindow(TimeSpan.FromMinutes(5), 5);
    private readonly IEventSubscriber<T> eventSubscriber;
    private readonly EventSubscriptionSource<T> eventSource;
    private SubscriptionHolder? currentSubscription;

    public int ReconnectWaitMs { get; set; } = 5000;

    public bool IsSubscribed => currentSubscription != null;

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
        Unsubscribe();
    }

    private void Subscribe()
    {
        lock (retryWindow)
        {
            if (currentSubscription != null)
            {
                return;
            }

            currentSubscription = new SubscriptionHolder(eventSource(this));
        }
    }

    private void Unsubscribe()
    {
        lock (retryWindow)
        {
            if (currentSubscription == null)
            {
                return;
            }

            currentSubscription.Dispose();
            currentSubscription = null;
        }
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
        if (!ReferenceEquals(subscription, currentSubscription?.Subscription))
        {
            return;
        }

        await eventSubscriber.OnNextAsync(this, @event);
    }

    async ValueTask IEventSubscriber<T>.OnErrorAsync(IEventSubscription subscription, Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            return;
        }

        if (!ReferenceEquals(subscription, currentSubscription?.Subscription))
        {
            return;
        }

        Unsubscribe();

        if (!retryWindow.CanRetryAfterFailure())
        {
            await eventSubscriber.OnErrorAsync(this, exception);
            return;
        }

        try
        {
            await Task.Delay(ReconnectWaitMs, currentSubscription?.Cancellation?.Token ?? default);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        Subscribe();
    }
}
