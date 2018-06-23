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

        Task StopAllAsync();

        Task StopAsync(string consumerName);

        Task StartAllAsync();

        Task StartAsync(string consumerName);

        Task ResetAsync(string consumerName);

        Task<Immutable<List<EventConsumerInfo>>> GetConsumersAsync();
    }
}
