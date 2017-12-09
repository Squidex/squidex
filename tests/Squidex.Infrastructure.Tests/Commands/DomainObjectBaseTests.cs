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

            var sut = new MyDomainObject();

            Assert.Equal(domainObjectId, sut.Id);
            Assert.Equal(domainObjectVersion, sut.Version);
        }

        [Fact]
        public void Should_add_event_to_uncommitted_events_and_increase_version_when_raised()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var sut = new MyDomainObject();

            sut.RaiseNewEvent(event1);
            sut.RaiseNewEvent(event2);

            Assert.Equal(12, sut.Version);

            Assert.Equal(new IEvent[] { event1, event2 }, sut.GetUncomittedEvents().Select(x => x.Payload).ToArray());

            sut.ClearUncommittedEvents();

            Assert.Equal(0, sut.GetUncomittedEvents().Count);
        }

        [Fact]
        public void Should_not_add_event_to_uncommitted_events_and_increase_version_when_raised()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            var sut = new MyDomainObject();

            sut.RaiseEvent(new Envelope<IEvent>(event1));
            sut.RaiseEvent(new Envelope<IEvent>(event2));

            Assert.Equal(12, sut.Version);
            Assert.Equal(0, sut.GetUncomittedEvents().Count);
        }
    }
}
