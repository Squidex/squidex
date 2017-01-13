// ==========================================================================
//  DefaultDomainObjectRepositoryTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class DefaultDomainObjectRepositoryTests
    {
        private readonly Mock<IDomainObjectFactory> factory = new Mock<IDomainObjectFactory>();
        private readonly Mock<IEventStore> eventStore = new Mock<IEventStore>();
        private readonly Mock<IEventPublisher> eventPublisher = new Mock<IEventPublisher>();
        private readonly Mock<IStreamNameResolver> streamNameResolver = new Mock<IStreamNameResolver>();
        private readonly Mock<EventDataFormatter> eventDataFormatter = new Mock<EventDataFormatter>(null);
        private readonly string streamName = Guid.NewGuid().ToString();
        private readonly Guid aggregateId = Guid.NewGuid();
        private readonly MyDomainObject domainObject;
        private readonly DefaultDomainObjectRepository sut;

        public DefaultDomainObjectRepositoryTests()
        {
            domainObject = new MyDomainObject(aggregateId, 123);

            streamNameResolver.Setup(x => x.GetStreamName(It.IsAny<Type>(), aggregateId)).Returns(streamName);

            factory.Setup(x => x.CreateNew(typeof(MyDomainObject), aggregateId)).Returns(domainObject);

            sut = new DefaultDomainObjectRepository(factory.Object, eventStore.Object, eventPublisher.Object, streamNameResolver.Object, eventDataFormatter.Object);
        }

        public sealed class MyEvent : IEvent
        {
        }

        public sealed class MyDomainObject : DomainObject
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
            eventStore.Setup(x => x.GetEventsAsync(streamName)).Returns(Observable.Empty<EventData>());

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetByIdAsync<MyDomainObject>(aggregateId));
        }

        [Fact]
        public async Task Should_apply_domain_objects_to_event()
        {
            var eventData1 = new EventData();
            var eventData2 = new EventData();

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            eventStore.Setup(x => x.GetEventsAsync(streamName)).Returns(new[] { eventData1, eventData2 }.ToObservable());

            eventDataFormatter.Setup(x => x.Parse(eventData1)).Returns(new Envelope<IEvent>(event1));
            eventDataFormatter.Setup(x => x.Parse(eventData2)).Returns(new Envelope<IEvent>(event2));

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

            eventStore.Setup(x => x.GetEventsAsync(streamName)).Returns(new[] { eventData1, eventData2 }.ToObservable());

            eventDataFormatter.Setup(x => x.Parse(eventData1)).Returns(new Envelope<IEvent>(event1));
            eventDataFormatter.Setup(x => x.Parse(eventData2)).Returns(new Envelope<IEvent>(event2));

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

            eventDataFormatter.Setup(x => x.ToEventData(It.Is<Envelope<IEvent>>(e => e.Payload == event1), commitId)).Returns(eventData1);
            eventDataFormatter.Setup(x => x.ToEventData(It.Is<Envelope<IEvent>>(e => e.Payload == event2), commitId)).Returns(eventData2);

            eventStore.Setup(x => x.AppendEventsAsync(commitId, streamName, 122, It.Is<IEnumerable<EventData>>(e => e.Count() == 2))).Returns(Task.FromResult(true)).Verifiable();

            domainObject.AddEvent(event1);
            domainObject.AddEvent(event2);

            await sut.SaveAsync(domainObject, domainObject.GetUncomittedEvents(), commitId);

            eventPublisher.Verify(x => x.Publish(eventData1));
            eventPublisher.Verify(x => x.Publish(eventData2));

            eventStore.VerifyAll();
        }

        [Fact]
        public async Task Should_throw_exception_on_version_mismatch()
        {
            var commitId = Guid.NewGuid();

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var eventData1 = new EventData();
            var eventData2 = new EventData();

            eventDataFormatter.Setup(x => x.ToEventData(It.Is<Envelope<IEvent>>(e => e.Payload == event1), commitId)).Returns(eventData1);
            eventDataFormatter.Setup(x => x.ToEventData(It.Is<Envelope<IEvent>>(e => e.Payload == event2), commitId)).Returns(eventData2);

            eventStore.Setup(x => x.AppendEventsAsync(commitId, streamName, 122, new List<EventData> { eventData1, eventData2 })).Throws(new WrongEventVersionException(1, 2)).Verifiable();

            domainObject.AddEvent(event1);
            domainObject.AddEvent(event2);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.SaveAsync(domainObject, domainObject.GetUncomittedEvents(), commitId));

            eventStore.VerifyAll();
        }
    }
}
