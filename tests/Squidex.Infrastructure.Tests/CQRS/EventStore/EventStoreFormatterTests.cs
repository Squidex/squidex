// ==========================================================================
//  EventStoreFormatterTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public class EventStoreFormatterTests
    {
        public sealed class Event : IEvent
        {
            public string MyProperty { get; set; }
        }

        public sealed class ReceivedEvent : IReceivedEvent
        {
            public int EventNumber { get; set; }

            public string EventType { get; set; }

            public byte[] Metadata { get; set; }

            public byte[] Payload { get; set; }

            public DateTime Created { get; set; }
        }

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();

        static EventStoreFormatterTests()
        {
            serializerSettings.Converters.Add(new PropertiesBagConverter());
        }

        public EventStoreFormatterTests()
        {
            TypeNameRegistry.Map(typeof(Event), "Event");
        }

        [Fact]
        public void Should_serialize_and_deserialize_envelope()
        {
            var commitId = Guid.NewGuid();
            var inputEvent = new Envelope<Event>(new Event { MyProperty = "My-Property" });

            inputEvent.SetAggregateId(Guid.NewGuid());
            inputEvent.SetAppId(Guid.NewGuid());
            inputEvent.SetCommitId(commitId);
            inputEvent.SetEventId(Guid.NewGuid());
            inputEvent.SetEventNumber(1);
            inputEvent.SetTimestamp(SystemClock.Instance.GetCurrentInstant());

            var sut = new EventStoreFormatter(serializerSettings);

            var eventData = sut.ToEventData(inputEvent.To<IEvent>(), commitId);

            var receivedEvent = new ReceivedEvent
            {
                Payload = eventData.Data,
                Created = inputEvent.Headers.Timestamp().ToDateTimeUtc(),
                EventNumber = 1,
                EventType = "event",
                Metadata = eventData.Metadata
            };

            var outputEvent = sut.Parse(receivedEvent).To<Event>();

            CompareHeaders(outputEvent.Headers, inputEvent.Headers);

            Assert.Equal(inputEvent.Payload.MyProperty, outputEvent.Payload.MyProperty);
        }

        private static void CompareHeaders(PropertiesBag lhs, PropertiesBag rhs)
        {
            foreach (var key in lhs.PropertyNames.Concat(rhs.PropertyNames).Distinct())
            {
                Assert.Equal(lhs[key].ToString(), rhs[key].ToString());
            }
        }
    }
}
