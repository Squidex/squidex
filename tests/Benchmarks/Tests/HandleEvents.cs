// ==========================================================================
//  HandleEvents.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Benchmarks.Tests.TestData;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.States;

namespace Benchmarks.Tests
{
    public sealed class HandleEvents : Benchmark
    {
        private const int NumEvents = 5000;
        private IServiceProvider services;
        private IEventStore eventStore;
        private IEventDataFormatter eventDataFormatter;
        private EventConsumerGrain eventConsumerGrain;
        private MyEventConsumer eventConsumer;

        public override void RunInitialize()
        {
            services = Services.Create();

            eventConsumer = new MyEventConsumer(NumEvents);

            eventStore = services.GetRequiredService<IEventStore>();

            eventDataFormatter = services.GetRequiredService<IEventDataFormatter>();
            eventConsumerGrain = services.GetRequiredService<EventConsumerGrain>();

            eventConsumerGrain.ActivateAsync("Test", services.GetRequiredService<IStore>()).Wait();
            eventConsumerGrain.Activate(eventConsumer);
        }

        public override long Run()
        {
            var streamName = Guid.NewGuid().ToString();

            for (var eventId = 0; eventId < NumEvents; eventId++)
            {
                var eventData = eventDataFormatter.ToEventData(new Envelope<IEvent>(new MyEvent { EventNumber = eventId + 1 }), Guid.NewGuid());

                eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, eventId - 1, new[] { eventData }).Wait();
            }

            eventConsumer.WaitAndVerify();

            return NumEvents;
        }

        public override void RunCleanup()
        {
            services.Cleanup();
        }
    }
}
