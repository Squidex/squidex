// ==========================================================================
//  EventConsumerRegistryGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Grains.Implementation
{
    public sealed class EventConsumerRegistryGrain : Grain<EventConsumerRegistryGrainState>, IEventConsumerRegistryGrain
    {
        public Task<List<EventConsumerInfo>> GetConsumersAsync()
        {
            var tasks =
                State.EventConsumerNames
                    .Select(n => GrainFactory.GetGrain<IEventConsumerGrain>(n))
                    .Select(c => c.GetStateAsync());

            return Task.WhenAll(tasks).ContinueWith(x => x.Result.ToList());
        }

        public Task RegisterAsync(string consumerName)
        {
            State.EventConsumerNames.Add(consumerName);

            return TaskHelper.Done;
        }

        public Task ResetAsync(string consumerName)
        {
            var eventConsumer = GrainFactory.GetGrain<IEventConsumerGrain>(consumerName);

            return eventConsumer.ResetAsync();
        }

        public Task StartAsync(string consumerName)
        {
            var eventConsumer = GrainFactory.GetGrain<IEventConsumerGrain>(consumerName);

            return eventConsumer.StartAsync();
        }

        public Task StopAsync(string consumerName)
        {
            var eventConsumer = GrainFactory.GetGrain<IEventConsumerGrain>(consumerName);

            return eventConsumer.StopAsync();
        }
    }
}
