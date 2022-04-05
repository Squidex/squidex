// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Concurrency;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerManagerGrain : IBackgroundGrain
    {
        Task ActivateAsync(string streamName);

        Task StartAllAsync();

        Task StopAllAsync();

        Task<EventConsumerInfo> StopAsync(string consumerName);

        Task<EventConsumerInfo> StartAsync(string consumerName);

        Task<EventConsumerInfo> ResetAsync(string consumerName);

        Task<Immutable<List<EventConsumerInfo>>> GetConsumersAsync();
    }
}
