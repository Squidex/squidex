// ==========================================================================
//  AppendToEventStoreWithManyWriters.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Benchmarks.Utils;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.EventSourcing;

namespace Benchmarks.Tests
{
    public sealed class AppendToEventStoreWithManyWriters : Benchmark
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
            const long numCommits = 200;
            const long numStreams = 100;

            Parallel.For(0, numStreams, streamId =>
            {
                var streamName = streamId.ToString();

                for (var commitId = 0; commitId < numCommits; commitId++)
                {
                    eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, new[] { Helper.CreateEventData() }).Wait();
                }
            });

            return numCommits * numStreams;
        }

        public override void RunCleanup()
        {
            services.Cleanup();
        }
    }
}
