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
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IPersistence<MyDomainState> persistence = A.Fake<IPersistence<MyDomainState>>();
        private readonly Guid id = Guid.NewGuid();
        private readonly MyDomainObject sut = new MyDomainObject();

        public DomainObjectBaseTests()
        {
            A.CallTo(() => store.WithSnapshots<MyDomainState>(id, A<Func<MyDomainState, Task>>.Ignored))
                .Returns(persistence);
        }

        [Fact]
        public void Should_instantiate()
        {
            Assert.Equal(EtagVersion.Empty, sut.Version);
        }

        [Fact]
        public void Should_add_event_to_uncommitted_events_and_increase_version_when_raised()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            sut.RaiseEvent(event1);
            sut.RaiseEvent(event2);

            Assert.Equal(1, sut.Version);
            Assert.Equal(new IEvent[] { event1, event2 }, sut.GetUncomittedEvents().Select(x => x.Payload).ToArray());

            sut.ClearUncommittedEvents();

            Assert.Equal(0, sut.GetUncomittedEvents().Count);
        }

        [Fact]
        public async Task Should_write_state_and_events_when_saved()
        {
            await sut.ActivateAsync(id, store);

            var event1 = new MyEvent();
            var event2 = new MyEvent();
            var newState = new MyDomainState();

            sut.RaiseEvent(event1);
            sut.RaiseEvent(event2);
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
            A.CallTo(() => persistence.WriteEventsAsync(A<IEnumerable<Envelope<IEvent>>>.Ignored))
                .Throws(new InvalidOperationException());

            await sut.ActivateAsync(id, store);

            var event1 = new MyEvent();
            var event2 = new MyEvent();
            var newState = new MyDomainState();

            sut.RaiseEvent(event1);
            sut.RaiseEvent(event2);
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
