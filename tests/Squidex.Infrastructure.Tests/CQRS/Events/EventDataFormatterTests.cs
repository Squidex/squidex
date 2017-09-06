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

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();

        public EventDataFormatterTests()
        {
            serializerSettings.Converters.Add(new PropertiesBagConverter());

            typeNameRegistry.Map(typeof(MyEvent), "Event");
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

            var sut = new EventDataFormatter(typeNameRegistry, serializerSettings);

            var eventData = sut.ToEventData(inputEvent.To<IEvent>(), commitId);

            var outputEvent = sut.Parse(eventData).To<MyEvent>();

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
