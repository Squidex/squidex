﻿// ==========================================================================
//  HandleEventsWithManyWriters.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Benchmarks.Tests.TestData;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.States;

namespace Benchmarks.Tests
{
    public sealed class HandleEventsWithManyWriters : Benchmark
    {
        private const int NumCommits = 200;
        private const int NumStreams = 10;
        private IServiceProvider services;
        private IEventStore eventStore;
        private IEventDataFormatter eventDataFormatter;
        private EventConsumerGrain eventConsumerGrain;
        private MyEventConsumer eventConsumer;

        public override void RunInitialize()
        {
            services = Services.Create();

            eventConsumer = new MyEventConsumer(NumStreams * NumCommits);

            eventStore = services.GetRequiredService<IEventStore>();
            eventDataFormatter = services.GetRequiredService<IEventDataFormatter>();

            eventConsumerGrain = services.GetRequiredService<EventConsumerGrain>();

            eventConsumerGrain.ActivateAsync("Test", services.GetRequiredService<IStore>()).Wait();
            eventConsumerGrain.Activate(eventConsumer);
        }

        public override long Run()
        {
            Parallel.For(0, NumStreams, streamId =>
            {
                var eventOffset = -1;
                var streamName = streamId.ToString();

                for (var commitId = 0; commitId < NumCommits; commitId++)
                {
                    var eventData = eventDataFormatter.ToEventData(new Envelope<IEvent>(new MyEvent()), Guid.NewGuid());

                    eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, eventOffset - 1, new[] { eventData }).Wait();
                    eventOffset++;
                }
            });

            eventConsumer.WaitAndVerify();

            return NumStreams * NumCommits;
        }

        public override void RunCleanup()
        {
            services.Cleanup();
        }
    }
}
