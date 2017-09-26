// ==========================================================================
//  EventDataFormatterTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class EventDataFormatterTests
    {
        public sealed class MyEvent : IEvent
        {
            public string MyProperty { get; set; }
        }

        public sealed class MyOldEvent : IEvent, IMigratedEvent
        {
            public string MyProperty { get; set; }

            public IEvent Migrate()
            {
                return new MyEvent { MyProperty = MyProperty };
            }
        }

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();
        private readonly EventDataFormatter sut;

        public EventDataFormatterTests()
        {
            serializerSettings.Converters.Add(new PropertiesBagConverter());

            typeNameRegistry.Map(typeof(MyEvent), "Event");
            typeNameRegistry.Map(typeof(MyOldEvent), "OldEvent");

            sut = new EventDataFormatter(typeNameRegistry, serializerSettings);
        }

        [Fact]
        public void Should_serialize_and_deserialize_envelope()
        {
            var commitId = Guid.NewGuid();

            var inputEvent = new Envelope<MyEvent>(new MyEvent { MyProperty = "My-Property" });

            inputEvent.SetAggregateId(Guid.NewGuid());
            inputEvent.SetCommitId(commitId);
            inputEvent.SetEventId(Guid.NewGuid());
            inputEvent.SetEventPosition("1");
            inputEvent.SetEventStreamNumber(1);
            inputEvent.SetTimestamp(SystemClock.Instance.GetCurrentInstant());

            var eventData = sut.ToEventData(inputEvent.To<IEvent>(), commitId);

            var outputEvent = sut.Parse(eventData).To<MyEvent>();

            AssertHeaders(inputEvent.Headers, outputEvent.Headers);
            AssertPayload(inputEvent, outputEvent);
        }

        [Fact]
        public void Should_migrate_event_serializing()
        {
            var inputEvent = new Envelope<MyOldEvent>(new MyOldEvent { MyProperty = "My-Property" });

            var eventData = sut.ToEventData(inputEvent.To<IEvent>(), Guid.NewGuid());

            var outputEvent = sut.Parse(eventData).To<MyEvent>();

            Assert.Equal(inputEvent.Payload.MyProperty, outputEvent.Payload.MyProperty);
        }

        [Fact]
        public void Should_migrate_event_deserializing()
        {
            var inputEvent = new Envelope<MyOldEvent>(new MyOldEvent { MyProperty = "My-Property" });

            var eventData = sut.ToEventData(inputEvent.To<IEvent>(), Guid.NewGuid(), false);

            var outputEvent = sut.Parse(eventData).To<MyEvent>();

            Assert.Equal(inputEvent.Payload.MyProperty, outputEvent.Payload.MyProperty);
        }

        private static void AssertPayload(Envelope<MyEvent> inputEvent, Envelope<MyEvent> outputEvent)
        {
            Assert.Equal(inputEvent.Payload.MyProperty, outputEvent.Payload.MyProperty);
        }

        private static void AssertHeaders(PropertiesBag lhs, PropertiesBag rhs)
        {
            foreach (var key in lhs.PropertyNames.Concat(rhs.PropertyNames).Distinct())
            {
                Assert.Equal(lhs[key].ToString(), rhs[key].ToString());
            }
        }
    }
}
