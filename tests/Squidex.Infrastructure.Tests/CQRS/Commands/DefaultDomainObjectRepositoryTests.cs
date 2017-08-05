// ==========================================================================
//  DefaultDomainObjectRepositoryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;
using Xunit;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class DefaultDomainObjectRepositoryTests
    {
        private readonly IDomainObjectFactory factory = A.Fake<IDomainObjectFactory>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly EventDataFormatter eventDataFormatter = A.Fake<EventDataFormatter>();
        private readonly string streamName = Guid.NewGuid().ToString();
        private readonly Guid aggregateId = Guid.NewGuid();
        private readonly MyDomainObject domainObject;
        private readonly DefaultDomainObjectRepository sut;

        public DefaultDomainObjectRepositoryTests()
        {
            domainObject = new MyDomainObject(aggregateId, 123);

            A.CallTo(() => streamNameResolver.GetStreamName(A<Type>.Ignored, aggregateId)).Returns(streamName);

            A.CallTo(() => factory.CreateNew(typeof(MyDomainObject), aggregateId)).Returns(domainObject);

            sut = new DefaultDomainObjectRepository(factory, eventStore, streamNameResolver, eventDataFormatter);
        }

        public sealed class MyEvent : IEvent
        {
        }

        public sealed class MyDomainObject : DomainObjectBase
        {
            private readonly List<IEvent> appliedEvents = new List<IEvent>();

            public List<IEvent> AppliedEvents
            {
                get { return appliedEvents; }
            }

            public MyDomainObject(Guid id, int version) : base(id, version)
            {
            }

            public void AddEvent(IEvent @event)
            {
                RaiseEvent(@event);
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
                appliedEvents.Add(@event.Payload);
            }
        }

        [Fact]
        public async Task Should_throw_exception_when_event_store_returns_no_events()
        {
            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(Task.FromResult<IReadOnlyList<StoredEvent>>(new List<StoredEvent>()));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetByIdAsync<MyDomainObject>(aggregateId));
        }

        [Fact]
        public async Task Should_apply_domain_objects_to_event()
        {
            var eventData1 = new EventData();
            var eventData2 = new EventData();

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var events = new List<StoredEvent>
            {
                new StoredEvent("0", 0, eventData1),
                new StoredEvent("1", 1, eventData2)
            };

            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(Task.FromResult<IReadOnlyList<StoredEvent>>(events));

            A.CallTo(() => eventDataFormatter.Parse(eventData1))
                .Returns(new Envelope<IEvent>(event1));
            A.CallTo(() => eventDataFormatter.Parse(eventData2))
                .Returns(new Envelope<IEvent>(event2));

            var result = await sut.GetByIdAsync<MyDomainObject>(aggregateId);

            Assert.Equal(result.AppliedEvents, new[] { event1, event2 });
        }

        [Fact]
        public async Task Should_throw_exception_if_final_version_does_not_match_to_expected()
        {
            var eventData1 = new EventData();
            var eventData2 = new EventData();

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var events = new List<StoredEvent>
            {
                new StoredEvent("0", 0, eventData1),
                new StoredEvent("1", 1, eventData2)
            };

            A.CallTo(() => eventStore.GetEventsAsync(streamName))
                .Returns(Task.FromResult<IReadOnlyList<StoredEvent>>(events));

            A.CallTo(() => eventDataFormatter.Parse(eventData1))
                .Returns(new Envelope<IEvent>(event1));
            A.CallTo(() => eventDataFormatter.Parse(eventData2))
                .Returns(new Envelope<IEvent>(event2));

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.GetByIdAsync<MyDomainObject>(aggregateId, 200));
        }

        [Fact]
        public async Task Should_append_events_and_publish()
        {
            var commitId = Guid.NewGuid();

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var eventData1 = new EventData();
            var eventData2 = new EventData();

            A.CallTo(() => eventDataFormatter.ToEventData(A<Envelope<IEvent>>.That.Matches(e => e.Payload == event1), commitId))
                .Returns(eventData1);
            A.CallTo(() => eventDataFormatter.ToEventData(A<Envelope<IEvent>>.That.Matches(e => e.Payload == event2), commitId))
                .Returns(eventData2);

            A.CallTo(() => eventStore.AppendEventsAsync(commitId, streamName, 123, A<ICollection<EventData>>.That.Matches(e => e.Count == 2)))
                .Returns(TaskHelper.Done);

            domainObject.AddEvent(event1);
            domainObject.AddEvent(event2);

            await sut.SaveAsync(domainObject, domainObject.GetUncomittedEvents(), commitId);

            A.CallTo(() => eventStore.AppendEventsAsync(commitId, streamName, 123, A<ICollection<EventData>>.That.Matches(e => e.Count == 2))).MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_on_version_mismatch()
        {
            var commitId = Guid.NewGuid();

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var eventData1 = new EventData();
            var eventData2 = new EventData();

            A.CallTo(() => eventDataFormatter.ToEventData(A<Envelope<IEvent>>.That.Matches(e => e.Payload == event1), commitId))
                .Returns(eventData1);
            A.CallTo(() => eventDataFormatter.ToEventData(A<Envelope<IEvent>>.That.Matches(e => e.Payload == event2), commitId))
                .Returns(eventData2);

            A.CallTo(() => eventStore.AppendEventsAsync(commitId, streamName, 123, A<ICollection<EventData>>.That.Matches(e => e.Count == 2)))
                .Throws(new WrongEventVersionException(1, 2));

            domainObject.AddEvent(event1);
            domainObject.AddEvent(event2);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.SaveAsync(domainObject, domainObject.GetUncomittedEvents(), commitId));

            A.CallTo(() => eventStore.AppendEventsAsync(commitId, streamName, 123, A<ICollection<EventData>>.That.Matches(e => e.Count == 2))).MustHaveHappened();
        }
    }
}
