// ==========================================================================
//  WrapperSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation
{
    internal sealed class WrapperSubscription : IEventSubscriber
    {
        private readonly IEventConsumerGrain grain;
        private readonly TaskScheduler scheduler;

        public WrapperSubscription(IEventConsumerGrain grain, TaskScheduler scheduler)
        {
            this.grain = grain;

            this.scheduler = scheduler ?? TaskScheduler.Default;
        }

        public Task OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
        {
            return Dispatch(() => grain.OnEventAsync(subscription.AsImmutable(), storedEvent.AsImmutable()));
        }

        public Task OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return Dispatch(() => grain.OnErrorAsync(subscription.AsImmutable(), exception.AsImmutable()));
        }

        public Task OnClosedAsync(IEventSubscription subscription)
        {
            return Dispatch(() => grain.OnClosedAsync(subscription.AsImmutable()));
        }

        private Task Dispatch(Func<Task> task)
        {
            return Task<Task>.Factory.StartNew(() => task(), CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();
        }
    }
}
