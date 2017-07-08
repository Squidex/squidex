// ==========================================================================
//  HandleEvents.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.EventStore;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf

namespace Benchmarks.Tests
{
    public sealed class HandleEvents : IBenchmark
    {
        [TypeName("MyEvent")]
        public sealed class MyEvent : IEvent
        {
            public int EventNumber { get; set; }
        }

        public sealed class MyEventConsumer : IEventConsumer
        {
            private readonly TaskCompletionSource<object> completion = new TaskCompletionSource<object>();
            private readonly int numEvents;

            public List<int> EventNumbers { get; } = new List<int>();

            public string Name
            {
                get { return typeof(MyEventConsumer).Name; }
            }

            public string EventsFilter
            {
                get { return string.Empty; }
            }

            public MyEventConsumer(int numEvents)
            {
                this.numEvents = numEvents;
            }

            public Task ClearAsync()
            {
                return TaskHelper.Done;
            }

            public void Wait()
            {
                completion.Task.Wait();
            }

            public Task On(Envelope<IEvent> @event)
            {
                if (@event.Payload is MyEvent myEvent)
                {
                    EventNumbers.Add(myEvent.EventNumber);

                    if (myEvent.EventNumber == numEvents)
                    {
                        completion.SetResult(true);
                    }
                }

                return TaskHelper.Done;
            }
        }

        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry().Map(typeof(MyEvent));
        private readonly EventDataFormatter formatter;
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private const int NumEvents = 5000;
        private IMongoClient mongoClient;
        private IMongoDatabase mongoDatabase;
        private IEventStore eventStore;
        private IEventNotifier eventNotifier;
        private IEventConsumerInfoRepository eventConsumerInfos;
        private EventReceiver eventReceiver;
        private MyEventConsumer eventConsumer;

        public string Id
        {
            get { return "handleEvents"; }
        }

        public string Name
        {
            get { return "HandleEvents"; }
        }

        public HandleEvents()
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
            eventNotifier = new DefaultEventNotifier(new InMemoryPubSub());
            eventStore = new MongoEventStore(mongoDatabase, eventNotifier);
            eventConsumer = new MyEventConsumer(NumEvents);

            eventReceiver = new EventReceiver(formatter, eventStore, eventNotifier, eventConsumerInfos, log);
            eventReceiver.Subscribe(eventConsumer);
        }

        public long Run()
        {
            var streamName = Guid.NewGuid().ToString();

            for (var eventId = 0; eventId < NumEvents; eventId++)
            {
                var eventData = formatter.ToEventData(new Envelope<IEvent>(new MyEvent { EventNumber = eventId + 1 }), Guid.NewGuid());

                eventStore.AppendEventsAsync(Guid.NewGuid(), streamName, eventId - 1, new [] { eventData }).Wait();
            }

            eventConsumer.Wait();

            if (eventConsumer.EventNumbers.Count != NumEvents)
            {
                throw new InvalidOperationException($"{eventConsumer.EventNumbers.Count} Events have been handled");
            }

            for (var i = 0; i < eventConsumer.EventNumbers.Count; i++)
            {
                var value = eventConsumer.EventNumbers[i];

                if (value != i + 1)
                {
                    throw new InvalidOperationException($"Event[{i}] != value");
                }
            }

            return NumEvents;
        }

        public void RunCleanup()
        {
            mongoClient.DropDatabase(mongoDatabase.DatabaseNamespace.DatabaseName);

            eventReceiver.Dispose();
        }

        public void Cleanup()
        {
        }
    }
}
