// ==========================================================================
//  HandleEventsWithManyWriters.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Benchmarks.Tests.TestData;
using Benchmarks.Utils;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Actors;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;

namespace Benchmarks.Tests
{
    public sealed class HandleEventsWithManyWriters : IBenchmark
    {
        private const int NumCommits = 200;
        private const int NumStreams = 10;
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry().Map(typeof(MyEvent));
        private readonly EventDataFormatter formatter;
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private IMongoClient mongoClient;
        private IMongoDatabase mongoDatabase;
        private IEventStore eventStore;
        private IEventNotifier eventNotifier;
        private IEventConsumerInfoRepository eventConsumerInfos;
        private EventConsumerActor eventConsumerActor;
        private MyEventConsumer eventConsumer;

        public string Id
        {
            get { return "handleEventsParallel"; }
        }

        public string Name
        {
            get { return "Handle events parallel"; }
        }

        public HandleEventsWithManyWriters()
        {
            serializerSettings.Converters.Add(new PropertiesBagConverter());

            formatter = new EventDataFormatter(typeNameRegistry, serializerSettings);
        }

        public void Initialize()
        {
            mongoClient = new MongoClient("mongodb://localhost");
        }

        public void RunInitialize()
        {
            mongoDatabase = mongoClient.GetDatabase(Guid.NewGuid().ToString());

            var log = new SemanticLog(new ILogChannel[0], new ILogAppender[0], () => new JsonLogWriter(Formatting.Indented, true));

            eventConsumerInfos = new MongoEventConsumerInfoRepository(mongoDatabase);
            eventConsumer = new MyEventConsumer(NumStreams * NumCommits);
            eventNotifier = new DefaultEventNotifier(new InMemoryPubSub());

            eventStore = new MongoEventStore(mongoDatabase, eventNotifier);

            eventConsumerActor = new EventConsumerActor(formatter, eventStore, eventConsumerInfos, log);
            eventConsumerActor.Subscribe(eventConsumer);
        }

        public long Run()
        {
            Parallel.For(0, NumStreams, streamId =>
            {
                var eventOffset = -1;
                var streamName = streamId.ToString();

                for (var commitId = 0; commitId < NumCommits; commitId++)
                {
                    var eventData = formatter.ToEventData(new Envelope<IEvent>(new MyEvent()), Guid.NewGuid());

                    eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, eventOffset - 1, new[] { eventData }).Wait();
                    eventOffset++;
                }
            });

            eventConsumer.WaitAndVerify();

            return NumStreams * NumCommits;
        }

        public void RunCleanup()
        {
            mongoClient.DropDatabase(mongoDatabase.DatabaseNamespace.DatabaseName);

            eventConsumerActor.Dispose();
        }

        public void Cleanup()
        {
        }
    }
}
