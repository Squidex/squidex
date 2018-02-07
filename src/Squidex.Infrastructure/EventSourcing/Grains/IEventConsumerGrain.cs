// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerGrain : IGrainWithStringKey
    {
        Task<Immutable<EventConsumerInfo>> GetStateAsync();

        Task ActivateAsync();

        Task StopAsync();

        Task StartAsync();

        Task ResetAsync();

        Task WakeUpAsync();

        Task OnEventAsync(Immutable<IEventSubscription> subscription, Immutable<StoredEvent> storedEvent);

        Task OnErrorAsync(Immutable<IEventSubscription> subscription, Immutable<Exception> exception);
    }
}
