// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerGrain : IBackgroundGrain
    {
        Task<Immutable<EventConsumerInfo>> GetStateAsync();

        Task StopAsync();

        Task StartAsync();

        Task ResetAsync();

        Task OnEventAsync(Immutable<IEventSubscription> subscription, Immutable<StoredEvent> storedEvent);

        Task OnErrorAsync(Immutable<IEventSubscription> subscription, Immutable<Exception> exception);
    }
}
