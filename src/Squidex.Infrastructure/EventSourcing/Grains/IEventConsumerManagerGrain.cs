// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerManagerGrain : IBackgroundGrain
    {
        Task ActivateAsync(string streamName);

        Task StartAllAsync();

        Task StopAllAsync();

        Task<Immutable<EventConsumerInfo>> StopAsync(string consumerName);

        Task<Immutable<EventConsumerInfo>> StartAsync(string consumerName);

        Task<Immutable<EventConsumerInfo>> ResetAsync(string consumerName);

        Task<Immutable<List<EventConsumerInfo>>> GetConsumersAsync();
    }
}
