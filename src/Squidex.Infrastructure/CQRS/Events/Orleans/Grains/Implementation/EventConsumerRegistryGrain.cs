// ==========================================================================
//  EventConsumerRegistryGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation
{
    public sealed class EventConsumerRegistryGrain : Grain, IEventConsumerRegistryGrain, IRemindable
    {
        private readonly IEnumerable<IEventConsumer> eventConsumers;

        public EventConsumerRegistryGrain(IEnumerable<IEventConsumer> eventConsumers)
        {
            Guard.NotNull(eventConsumers, nameof(eventConsumers));

            this.eventConsumers = eventConsumers;
        }

        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterTimer(x => ActivateAsync(null), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.FromResult(true);
        }

        public Task ActivateAsync(string streamName)
        {
            var tasks =
                eventConsumers
                    .Where(c => streamName == null || Regex.IsMatch(streamName, c.EventsFilter))
                    .Select(c => GrainFactory.GetGrain<IEventConsumerGrain>(c.Name))
                    .Select(c => c.ActivateAsync());

            return Task.WhenAll(tasks);
        }

        public Task<Immutable<List<EventConsumerInfo>>> GetConsumersAsync()
        {
            var tasks =
                eventConsumers
                    .Select(c => GrainFactory.GetGrain<IEventConsumerGrain>(c.Name))
                    .Select(c => c.GetStateAsync());

            return Task.WhenAll(tasks).ContinueWith(x => new Immutable<List<EventConsumerInfo>>(x.Result.Select(r => r.Value).ToList()));
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
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
