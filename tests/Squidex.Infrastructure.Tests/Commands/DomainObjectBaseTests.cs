// ==========================================================================
//  DomainObjectBaseTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Infrastructure.Commands.TestHelpers;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class DomainObjectBaseTests
    {
        [Fact]
        public void Should_instantiate()
        {
            var domainObjectId = Guid.NewGuid();
            var domainObjectVersion = 123;

            var sut = new MyDomainObject(domainObjectId, domainObjectVersion);

            Assert.Equal(domainObjectId, sut.Id);
            Assert.Equal(domainObjectVersion, sut.Version);
        }

        [Fact]
        public void Should_add_event_to_uncommitted_events_and_increase_version_when_raised()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var sut = new MyDomainObject(Guid.NewGuid(), 10);

            IAggregate aggregate = sut;

            sut.RaiseNewEvent(event1);
            sut.RaiseNewEvent(event2);

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

            var sut = new MyDomainObject(Guid.NewGuid(), 10);

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

            var user1a = new MyDomainObject(id1, 1);
            var user1b = new MyDomainObject(id1, 2);
            var user2a = new MyDomainObject(id2, 2);

            Assert.True(user1a.Equals(user1b));
            Assert.False(user1a.Equals(user2a));
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var user1a = new MyDomainObject(id1, 1);

            object user1b = new MyDomainObject(id1, 2);
            object user2a = new MyDomainObject(id2, 2);

            Assert.True(user1a.Equals(user1b));
            Assert.False(user1a.Equals(user2a));
        }

        [Fact]
        public void Should_provide_correct_hash_codes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var user1a = new MyDomainObject(id1, 1);
            var user1b = new MyDomainObject(id1, 2);
            var user2a = new MyDomainObject(id2, 2);

            Assert.Equal(user1a.GetHashCode(), user1b.GetHashCode());
            Assert.NotEqual(user1a.GetHashCode(), user2a.GetHashCode());
        }
    }
}
