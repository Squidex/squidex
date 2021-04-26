// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using NodaTime;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class DefaultEventDataFormatterTests
    {
        public sealed class MyOldEvent : IEvent, IMigrated<IEvent>
        {
            public string MyProperty { get; set; }

            public IEvent Migrate()
            {
                return new MyEvent { MyProperty = MyProperty };
            }
        }

        private readonly DefaultEventDataFormatter sut;

        public DefaultEventDataFormatterTests()
        {
            var typeNameRegistry =
                new TypeNameRegistry()
                    .Map(typeof(MyEvent), "Event")
                    .Map(typeof(MyOldEvent), "OldEvent");

            sut = new DefaultEventDataFormatter(typeNameRegistry, TestUtils.CreateSerializer(typeNameRegistry));
        }

        [Fact]
        public void Should_serialize_and_deserialize_envelope()
        {
            var commitId = Guid.NewGuid();

            var inputEvent = new Envelope<MyEvent>(new MyEvent { MyProperty = "My-Property" });

            inputEvent.SetAggregateId(DomainId.NewGuid());
            inputEvent.SetCommitId(commitId);
            inputEvent.SetEventId(Guid.NewGuid());
            inputEvent.SetEventPosition("1");
            inputEvent.SetEventStreamNumber(1);
            inputEvent.SetTimestamp(SystemClock.Instance.GetCurrentInstant());

            var eventData = sut.ToEventData(inputEvent, commitId);
            var eventStored = new StoredEvent("stream", "0", -1, eventData);

            var outputEvent = sut.Parse(eventStored).To<MyEvent>();

            AssertHeaders(inputEvent.Headers, outputEvent.Headers);
            AssertPayload(inputEvent, outputEvent);
        }

        [Fact]
        public void Should_migrate_event_serializing()
        {
            var inputEvent = new Envelope<MyOldEvent>(new MyOldEvent { MyProperty = "My-Property" });

            var eventData = sut.ToEventData(inputEvent, Guid.NewGuid());
            var eventStored = new StoredEvent("stream", "0", -1, eventData);

            var outputEvent = sut.Parse(eventStored).To<MyEvent>();

            Assert.Equal(inputEvent.Payload.MyProperty, outputEvent.Payload.MyProperty);
        }

        [Fact]
        public void Should_migrate_event_deserializing()
        {
            var inputEvent = new Envelope<MyOldEvent>(new MyOldEvent { MyProperty = "My-Property" });

            var eventData = sut.ToEventData(inputEvent, Guid.NewGuid(), false);
            var eventStored = new StoredEvent("stream", "0", -1, eventData);

            var outputEvent = sut.Parse(eventStored).To<MyEvent>();

            Assert.Equal(inputEvent.Payload.MyProperty, outputEvent.Payload.MyProperty);
        }

        private static void AssertPayload(Envelope<MyEvent> inputEvent, Envelope<MyEvent> outputEvent)
        {
            Assert.Equal(inputEvent.Payload.MyProperty, outputEvent.Payload.MyProperty);
        }

        private static void AssertHeaders(EnvelopeHeaders lhs, EnvelopeHeaders rhs)
        {
            foreach (var key in lhs.Keys.Concat(rhs.Keys).Distinct())
            {
                Assert.Equal(lhs[key].ToString(), rhs[key].ToString());
            }
        }
    }
}
