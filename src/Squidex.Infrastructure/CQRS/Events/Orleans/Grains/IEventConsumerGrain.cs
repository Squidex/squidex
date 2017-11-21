// ==========================================================================
//  IEventConsumerGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains
{
    public interface IEventConsumerGrain : IGrainWithStringKey
    {
        Task<Immutable<EventConsumerInfo>> GetStateAsync();

        Task ActivateAsync();

        Task StopAsync();

        Task StartAsync();

        Task ResetAsync();

        Task OnEventAsync(Immutable<IEventSubscription> subscription, Immutable<StoredEvent> storedEvent);

        Task OnErrorAsync(Immutable<IEventSubscription> subscription, Immutable<Exception> exception);

        Task OnClosedAsync(Immutable<IEventSubscription> subscription);
    }
}
