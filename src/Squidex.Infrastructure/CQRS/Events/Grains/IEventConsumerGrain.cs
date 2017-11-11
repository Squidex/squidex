// ==========================================================================
//  IEventConsumerGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.CQRS.Events.Grains
{
    public interface IEventConsumerGrain : IGrainWithStringKey, IEventSubscriber
    {
        Task<EventConsumerInfo> GetStateAsync();

        Task StopAsync();

        Task StartAsync();

        Task ResetAsync();
    }
}
