// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using FluentAssertions;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Messaging;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerManagerTests
    {
        private readonly IPersistenceFactory<EventConsumerState> persistenceFactory = A.Fake<IPersistenceFactory<EventConsumerState>>();
        private readonly IMessageBus messaging = A.Fake<IMessageBus>();
        private readonly string consumerName = Guid.NewGuid().ToString();
        private readonly EventConsumerManager sut;

        public EventConsumerManagerTests()
        {
            sut = new EventConsumerManager(persistenceFactory, messaging);
        }

        [Fact]
        public async Task Should_get_states_from_store()
        {
            var snapshotStore = A.Fake<ISnapshotStore<EventConsumerState>>();

            A.CallTo(() => persistenceFactory.Snapshots)
                .Returns(snapshotStore);

            A.CallTo(() => snapshotStore.ReadAllAsync(default))
                .Returns(new List<SnapshotResult<EventConsumerState>>
                {
                    new SnapshotResult<EventConsumerState>(DomainId.Create("consumer1"),
                        new EventConsumerState
                        {
                            Position = "1"
                        }, 1),
                    new SnapshotResult<EventConsumerState>(DomainId.Create("consumer2"),
                        new EventConsumerState
                        {
                            Position = "2"
                        }, 2)
                }.ToAsyncEnumerable());

            var result = await sut.GetConsumersAsync(default);

            result.Should().BeEquivalentTo(
                new List<EventConsumerInfo>
                {
                    new EventConsumerInfo { Name = "consumer1", Position = "1" },
                    new EventConsumerInfo { Name = "consumer2", Position = "2" }
                });
        }

        [Fact]
        public async Task Should_publish_event_on_start()
        {
            var testState = new TestState<EventConsumerState>(DomainId.Create(consumerName), persistenceFactory)
            {
                Value = new EventConsumerState
                {
                    Position = "42"
                }
            };

            var response = await sut.StartAsync(consumerName, default);

            A.CallTo(() => messaging.PublishAsync(new EventConsumerStart(consumerName), null, default))
                .MustHaveHappened();

            Assert.Equal("42", response.Position);
        }

        [Fact]
        public async Task Should_publish_event_on_stop()
        {
            var testState = new TestState<EventConsumerState>(DomainId.Create(consumerName), persistenceFactory)
            {
                Value = new EventConsumerState
                {
                    Position = "42"
                }
            };

            var response = await sut.StopAsync(consumerName, default);

            A.CallTo(() => messaging.PublishAsync(new EventConsumerStop(consumerName), null, default))
                .MustHaveHappened();

            Assert.Equal("42", response.Position);
        }

        [Fact]
        public async Task Should_publish_event_on_reset()
        {
            var testState = new TestState<EventConsumerState>(DomainId.Create(consumerName), persistenceFactory)
            {
                Value = new EventConsumerState
                {
                    Position = "42"
                }
            };

            var response = await sut.ResetAsync(consumerName, default);

            A.CallTo(() => messaging.PublishAsync(new EventConsumerReset(consumerName), null, default))
                .MustHaveHappened();

            Assert.Equal("42", response.Position);
        }
    }
}
