// ==========================================================================
//  DomainObjectBaseTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class DomainObjectBaseTests
    {
        private readonly IStore store = A.Fake<IStore>();
        private readonly IPersistence<object> persistence = A.Fake<IPersistence<object>>();
        private readonly Guid id = Guid.NewGuid();
        private readonly MyDomainObject sut = new MyDomainObject();

        public DomainObjectBaseTests()
        {
            A.CallTo(() => store.WithSnapshots<MyDomainObject, object>(id.ToString(), A<Func<object, Task>>.Ignored))
                .Returns(persistence);
        }

        [Fact]
        public void Should_instantiate()
        {
            Assert.Equal(EtagVersion.NotFound, sut.Version);
        }

        [Fact]
        public void Should_add_event_to_uncommitted_events_and_not_increase_version_when_raised()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            sut.RaiseEvent(event1);
            sut.RaiseEvent(event2);

            Assert.Equal(EtagVersion.NotFound, sut.Version);
            Assert.Equal(new IEvent[] { event1, event2 }, sut.GetUncomittedEvents().Select(x => x.Payload).ToArray());

            sut.ClearUncommittedEvents();

            Assert.Equal(0, sut.GetUncomittedEvents().Count);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_saved()
        {
            A.CallTo(() => persistence.Version)
                .Returns(100);

            await sut.ActivateAsync(id.ToString(), store);

            Assert.Equal(100, sut.Version);

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            sut.RaiseEvent(event1);
            sut.RaiseEvent(event2);

            var newState = new MyDomainState();

            sut.UpdateState(newState);

            await sut.WriteAsync(A.Fake<ISemanticLog>());

            A.CallTo(() => persistence.WriteSnapshotAsync(newState))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 2)))
                .MustHaveHappened();

            Assert.Empty(sut.GetUncomittedEvents());
        }

        [Fact]
        public async Task Should_ignore_exception_when_saving()
        {
            A.CallTo(() => persistence.Version)
                .Returns(100);

            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .Throws(new InvalidOperationException());

            await sut.ActivateAsync(id.ToString(), store);

            Assert.Equal(100, sut.Version);

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            sut.RaiseEvent(event1);
            sut.RaiseEvent(event2);

            var newState = new MyDomainState();

            sut.UpdateState(newState);

            await sut.WriteAsync(A.Fake<ISemanticLog>());

            A.CallTo(() => persistence.WriteSnapshotAsync(newState))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.That.Matches(x => x.Count() == 2)))
                .MustHaveHappened();

            Assert.Empty(sut.GetUncomittedEvents());
        }
    }
}
