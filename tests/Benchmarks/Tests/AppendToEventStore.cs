// ==========================================================================
//  AppendToEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Benchmarks.Utils;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Benchmarks.Tests
{
    public sealed class AppendToEventStore : IBenchmark
    {
        private IMongoClient mongoClient;
        private IMongoDatabase mongoDatabase;
        private IEventStore eventStore;

        public string Id
        {
            get { return "appendToEventStore"; }
        }

        public string Name
        {
            get { return "Append events"; }
        }

        public void Initialize()
        {
            mongoClient = new MongoClient("mongodb://localhost");
        }

        public void RunInitialize()
        {
            mongoDatabase = mongoClient.GetDatabase(Guid.NewGuid().ToString());

            eventStore = new MongoEventStore(mongoDatabase, new DefaultEventNotifier(new InMemoryPubSub()));
        }

        public long Run()
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

        public void RunCleanup()
        {
            mongoClient.DropDatabase(mongoDatabase.DatabaseNamespace.DatabaseName);
        }

        public void Cleanup()
        {
        }
    }
}
