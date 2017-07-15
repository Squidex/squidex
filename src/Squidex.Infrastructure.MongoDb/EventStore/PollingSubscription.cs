// ==========================================================================
//  PollingSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.MongoDb.EventStore
{
    public sealed class PollingSubscription : DisposableObjectBase, IEventSubscription
    {
        private readonly IEventNotifier eventNotifier;
        private readonly MongoEventStore eventStore;
        private readonly string streamFilter;
        private readonly string position;
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
                timer.Dispose();
            }
        }

        public Task SubscribeAsync(Func<StoredEvent, Task> onNext, Func<Exception, Task> onError = null)
        {
            Guard.NotNull(onNext, nameof(onNext));

            if (timer == null)
            {
                throw new InvalidOperationException("An handler has already been registered.");
            }

            timer = new CompletionTimer(5000, async ct =>
            {
                try
                {
                    await eventStore.GetEventsAsync(onNext, ct, streamFilter, position);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            });

            eventNotifier.Subscribe(timer.Wakeup);

            return TaskHelper.Done;
        }
    }
}
