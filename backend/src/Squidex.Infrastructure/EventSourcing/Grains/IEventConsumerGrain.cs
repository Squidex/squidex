// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans.Concurrency;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerGrain : IBackgroundGrain
    {
        Task<Immutable<EventConsumerInfo>> GetStateAsync();

        Task<Immutable<EventConsumerInfo>> StopAsync();

        Task<Immutable<EventConsumerInfo>> StartAsync();

        Task<Immutable<EventConsumerInfo>> ResetAsync();
    }
}
