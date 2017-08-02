// ==========================================================================
//  PollingSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class PollingSubscription : DisposableObjectBase, IEventSubscription
    {
        private readonly IEventNotifier eventNotifier;
        private readonly MongoEventStore eventStore;
        private readonly string streamFilter;
        private string position;
        private bool isStopped;
        private IDisposable subscription;
        private CompletionTimer timer;

        public PollingSubscription(MongoEventStore eventStore, IEventNotifier eventNotifier, string streamFilter, string position)
        {
            this.position = position;
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
            this.streamFilter = streamFilter;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                isStopped = true;

                subscription?.Dispose();

                timer?.StopAsync().Forget();
            }
        }

        public Task SubscribeAsync(Func<StoredEvent, Task> onNext, Func<Exception, Task> onError = null)
        {
            Guard.NotNull(onNext, nameof(onNext));

            if (timer != null)
            {
                throw new InvalidOperationException("An handler has already been registered.");
            }

            timer = new CompletionTimer(5000, async ct =>
            {
                try
                {
                    await eventStore.GetEventsAsync(async storedEvent =>
                    {
                        if (!isStopped)
                        {
                            await onNext(storedEvent);

                            position = storedEvent.EventPosition;
                        }
                    }, ct, streamFilter, position);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    if (!isStopped)
                    {
                        onError?.Invoke(ex);
                    }
                }
            });

            subscription = eventNotifier.Subscribe(() =>
            {
                if (!isStopped)
                {
                    timer.SkipCurrentDelay();
                }
            });

            return TaskHelper.Done;
        }
    }
}
