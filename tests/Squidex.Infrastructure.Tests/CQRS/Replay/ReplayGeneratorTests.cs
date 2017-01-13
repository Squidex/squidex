// ==========================================================================
//  ReplayGeneratorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Replay
{
    public class ReplayGeneratorTests
    {
        private readonly Mock<IEventStore> eventStore = new Mock<IEventStore>();
        private readonly Mock<IEventPublisher> eventPublisher = new Mock<IEventPublisher>();
        private readonly Mock<IReplayableStore> store1 = new Mock<IReplayableStore>();
        private readonly Mock<IReplayableStore> store2 = new Mock<IReplayableStore>();
        private readonly Mock<ILogger<ReplayGenerator>> logger = new Mock<ILogger<ReplayGenerator>>();
        private readonly ReplayGenerator sut;
        
        public ReplayGeneratorTests()
        {
            sut = new ReplayGenerator(logger.Object, eventStore.Object, eventPublisher.Object, new[] { store1.Object, store2.Object });
        }

        [Fact]
        public async Task Should_clear_stores_and_replay_events()
        {
            var event1 = new EventData();
            var event2 = new EventData();
            var event3 = new EventData();
            
            eventStore.Setup(x => x.GetEventsAsync()).Returns(new[] { event1, event2, event3 }.ToObservable());

            await sut.ReplayAllAsync();

            store1.Verify(x => x.ClearAsync(), Times.Once());
            store2.Verify(x => x.ClearAsync(), Times.Once());

            eventPublisher.Verify(x => x.Publish(event1));
            eventPublisher.Verify(x => x.Publish(event2));
            eventPublisher.Verify(x => x.Publish(event3));
        }

        [Fact]
        public async Task Should_not_publish_if_clearing_failed()
        {
            var event1 = new EventData();
            var event2 = new EventData();
            var event3 = new EventData();

            store1.Setup(x => x.ClearAsync()).Throws(new InvalidOperationException());

            eventStore.Setup(x => x.GetEventsAsync()).Returns(new[] { event1, event2, event3 }.ToObservable());

            await sut.ReplayAllAsync();

            store1.Verify(x => x.ClearAsync(), Times.Once());
            store2.Verify(x => x.ClearAsync(), Times.Never());

            eventStore.Verify(x => x.GetEventsAsync(), Times.Never());
            eventPublisher.Verify(x => x.Publish(It.IsAny<EventData>()), Times.Never());
        }

        [Fact]
        public async Task Should_not_throw_if_process_throws()
        {
            eventStore.Setup(x => x.GetEventsAsync()).Throws(new InvalidOperationException());

            await sut.ReplayAllAsync();
            
            eventPublisher.Verify(x => x.Publish(It.IsAny<EventData>()), Times.Never());
        }
    }
}
