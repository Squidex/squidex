// ==========================================================================
//  AppendToEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb.EventStore;

namespace Benchmarks.Tests
{
    public sealed class AppendToEventStore : IBenchmark
    {
        private IMongoClient mongoClient;
        private IMongoDatabase mongoDatabase;

        private static readonly EventData EventData = new EventData
        {
            EventId = Guid.NewGuid(),
            Metadata = "EventMetdata",
            Payload = "EventPayload",
            Type = "MyEvent"
        };

        public string Id
        {
            get { return "appendToEventStore"; }
        }

        public string Name
        {
            get { return "Append Events to EventStore"; }
        }

        public void Initialize()
        {
            mongoClient = new MongoClient("mongodb://localhost");
        }

        public void RunInitialize()
        {
            mongoDatabase = mongoClient.GetDatabase(Guid.NewGuid().ToString());
            mongoDatabase.CreateCollection("Test");
        }

        public long Run()
        {
            const long numCommits = 10;
            const long eventStreams = 10;

            var eventStore = new MongoEventStore(mongoDatabase, new DefaultEventNotifier(new InMemoryPubSub()), SystemClock.Instance);

            for (var streamId = 0; streamId < eventStreams; streamId++)
            {
                var eventOffset = -1;
                var streamName = streamId.ToString();

                for (var commitId = 0; commitId < numCommits; commitId++)
                {
                    eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, eventOffset, new[] { EventData }).Wait();

                    eventOffset++;
                }
            }

            return numCommits * eventStreams;
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
