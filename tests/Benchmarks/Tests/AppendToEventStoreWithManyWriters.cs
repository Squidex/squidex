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
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Benchmarks.Tests
{
    public sealed class AppendToEventStoreWithManyWriters : IBenchmark
    {
        private IMongoClient mongoClient;
        private IMongoDatabase mongoDatabase;
        private IEventStore eventStore;

        public string Id
        {
            get { return "appendToEventStoreParallel"; }
        }

        public string Name
        {
            get { return "Append events parallel"; }
        }

        public void Initialize()
        {
            mongoClient = new MongoClient("mongodb://localhost");
        }

        public void RunInitialize()
        {
            mongoDatabase = mongoClient.GetDatabase(Guid.NewGuid().ToString());

            eventStore = new MongoEventStore(mongoDatabase, new DefaultEventNotifier(new InMemoryPubSub()));
            eventStore.Warmup();
        }

        public long Run()
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

        public void RunCleanup()
        {
            mongoClient.DropDatabase(mongoDatabase.DatabaseNamespace.DatabaseName);
        }

        public void Cleanup()
        {
        }
    }
}
