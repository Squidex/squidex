// ==========================================================================
//  AppendToEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Benchmarks.Utils;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.CQRS.Events;

namespace Benchmarks.Tests
{
    public sealed class AppendToEventStore : Benchmark
    {
        private IServiceProvider services;
        private IEventStore eventStore;

        public override void RunInitialize()
        {
            services = Services.Create();

            eventStore = services.GetRequiredService<IEventStore>();
        }

        public override long Run()
        {
            const long numCommits = 100;
            const long numStreams = 20;

            for (var streamId = 0; streamId < numStreams; streamId++)
            {
                var eventOffset = -1;
                var streamName = streamId.ToString();

                for (var commitId = 0; commitId < numCommits; commitId++)
                {
                    eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, eventOffset, new[] { Helper.CreateEventData() }).Wait();
                    eventOffset++;
                }
            }

            return numCommits * numStreams;
        }

        public override void RunCleanup()
        {
            services.Cleanup();
        }
    }
}
