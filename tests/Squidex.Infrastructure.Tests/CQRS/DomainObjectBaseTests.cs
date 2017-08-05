// ==========================================================================
//  DomainObjectBaseTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Infrastructure.CQRS
{
    public class DomainObjectBaseTests
    {
        private sealed class MyEvent : IEvent
        {
        }

        private sealed class DO : DomainObjectBase
        {
            public DO(Guid id, int version)
                : base(id, version)
            {
            }

            public void RaiseTestEvent(IEvent @event)
            {
                RaiseEvent(@event);
            }

            protected override void DispatchEvent(Envelope<IEvent> @event)
            {
            }
        }

        [Fact]
        public void Should_instantiate()
        {
            var id = Guid.NewGuid();
            var ver = 123;
            var sut = new DO(id, ver);

            Assert.Equal(id, sut.Id);
            Assert.Equal(ver, sut.Version);
        }

        [Fact]
        public void Should_add_event_to_uncommitted_events_and_increase_version_when_raised()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var sut = new DO(Guid.NewGuid(), 10);

            IAggregate aggregate = sut;

            sut.RaiseTestEvent(event1);
            sut.RaiseTestEvent(event2);

            Assert.Equal(12, sut.Version);

            Assert.Equal(new IEvent[] { event1, event2 }, aggregate.GetUncomittedEvents().Select(x => x.Payload).ToArray());

            aggregate.ClearUncommittedEvents();

            Assert.Equal(0, sut.GetUncomittedEvents().Count);
        }

        [Fact]
        public void Should_not_add_event_to_uncommitted_events_and_increase_version_when_raised()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var sut = new DO(Guid.NewGuid(), 10);

            IAggregate aggregate = sut;

            aggregate.ApplyEvent(new Envelope<IEvent>(event1));
            aggregate.ApplyEvent(new Envelope<IEvent>(event2));

            Assert.Equal(12, sut.Version);
            Assert.Equal(0, sut.GetUncomittedEvents().Count);
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var user1a = new DO(id1, 1);
            var user1b = new DO(id1, 2);
            var user2 = new DO(id2, 2);

            Assert.True(user1a.Equals(user1b));

            Assert.False(user1a.Equals(user2));
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var user1a = new DO(id1, 1);

            object user1b = new DO(id1, 2);
            object user2  = new DO(id2, 2);

            Assert.True(user1a.Equals(user1b));

            Assert.False(user1a.Equals(user2));
        }

        [Fact]
        public void Should_provide_correct_hash_codes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var user1a = new DO(id1, 1);
            var user1b = new DO(id1, 2);
            var user2 =  new DO(id2, 2);

            Assert.Equal(user1a.GetHashCode(), user1b.GetHashCode());

            Assert.NotEqual(user1a.GetHashCode(), user2.GetHashCode());
        }
    }
}
