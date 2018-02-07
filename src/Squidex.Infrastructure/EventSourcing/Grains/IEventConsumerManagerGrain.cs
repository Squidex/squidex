// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerManagerGrain : IGrainWithStringKey
    {
        Task ActivateAsync();

        Task WakeUpAsync(string streamName);

        Task StopAsync(string consumerName);

        Task StartAsync(string consumerName);

        Task ResetAsync(string consumerName);

        Task<Immutable<List<EventConsumerInfo>>> GetConsumersAsync();
    }
}
