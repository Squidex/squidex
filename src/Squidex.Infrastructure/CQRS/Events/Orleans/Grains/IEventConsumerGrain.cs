// ==========================================================================
//  IEventConsumerGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains
{
    public interface IEventConsumerGrain : IGrainWithStringKey, IEventSubscriber
    {
        Task<Immutable<EventConsumerInfo>> GetStateAsync();

        Task ActivateAsync();

        Task StopAsync();

        Task StartAsync();

        Task ResetAsync();
    }
}
