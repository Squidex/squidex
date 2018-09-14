// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class PollingSubscription : IEventSubscription
    {
        private readonly CompletionTimer timer;

        public PollingSubscription(
            IEventStore eventStore,
            IEventSubscriber eventSubscriber,
            string streamFilter,
            string position)
        {
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventSubscriber, nameof(eventSubscriber));

            timer = new CompletionTimer(5000, async ct =>
            {
                try
                {
                    await eventStore.QueryAsync(async storedEvent =>
                    {
                        await eventSubscriber.OnEventAsync(this, storedEvent);

                        position = storedEvent.EventPosition;
                    }, streamFilter, position, ct);
                }
                catch (Exception ex)
                {
                    if (!ex.Is<OperationCanceledException>())
                    {
                        await eventSubscriber.OnErrorAsync(this, ex);
                    }
                }
            });
        }

        public void WakeUp()
        {
            timer.SkipCurrentDelay();
        }

        public Task StopAsync()
        {
            return timer.StopAsync();
        }
    }
}
