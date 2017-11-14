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
using Orleans.Core;
using Orleans.Runtime;

namespace Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation
{
    public class EventConsumerRegistryGrain : Grain, IEventConsumerRegistryGrain, IRemindable
    {
        private readonly IEnumerable<IEventConsumer> eventConsumers;

        public EventConsumerRegistryGrain(IEnumerable<IEventConsumer> eventConsumers)
            : this(eventConsumers, null, null)
        {
        }

        protected EventConsumerRegistryGrain(
            IEnumerable<IEventConsumer> eventConsumers,
            IGrainIdentity identity,
            IGrainRuntime runtime)
            : base(identity, runtime)
        {
            Guard.NotNull(eventConsumers, nameof(eventConsumers));

            this.eventConsumers = eventConsumers;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return ActivateAsync(null);
        }

        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));
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
