// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Messaging;

namespace Squidex.Infrastructure.EventSourcing.Consume;

public class EventConsumerManagerTests
{
    private readonly IPersistenceFactory<EventConsumerState> persistenceFactory = A.Fake<IPersistenceFactory<EventConsumerState>>();
    private readonly IMessageBus messaging = A.Fake<IMessageBus>();
    private readonly string consumerName1 = Guid.NewGuid().ToString();
    private readonly string consumerName2 = Guid.NewGuid().ToString();
    private readonly EventConsumerManager sut;

    public EventConsumerManagerTests()
    {
        var consumer1 = A.Fake<IEventConsumer>();
        var consumer2 = A.Fake<IEventConsumer>();

        A.CallTo(() => consumer1.Name)
            .Returns(consumerName1);

        A.CallTo(() => consumer2.Name)
            .Returns(consumerName2);

        sut = new EventConsumerManager(persistenceFactory, new[] { consumer1, consumer2 }, messaging);
    }

    [Fact]
    public async Task Should_get_states_from_store_without_old_consumer()
    {
        var snapshotStore = A.Fake<ISnapshotStore<EventConsumerState>>();

        A.CallTo(() => persistenceFactory.Snapshots)
            .Returns(snapshotStore);

        A.CallTo(() => snapshotStore.ReadAllAsync(default))
            .Returns(new List<SnapshotResult<EventConsumerState>>
            {
                new SnapshotResult<EventConsumerState>(DomainId.Create(consumerName1),
                    new EventConsumerState
                    {
                        Position = "1"
                    }, 1),
                new SnapshotResult<EventConsumerState>(DomainId.Create(consumerName2),
                    new EventConsumerState
                    {
                        Position = "2"
                    }, 2),
                new SnapshotResult<EventConsumerState>(DomainId.Create("oldConsumer"),
                    new EventConsumerState
                    {
                        Position = "2"
                    }, 2)
            }.ToAsyncEnumerable());

        var actual = await sut.GetConsumersAsync(default);

        actual.Should().BeEquivalentTo(
            new List<EventConsumerInfo>
            {
                new EventConsumerInfo { Name = consumerName1, Position = "1" },
                new EventConsumerInfo { Name = consumerName2, Position = "2" }
            });
    }

    [Fact]
    public async Task Should_throw_exception_when_calling_old_consumer()
    {
        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.StartAsync("oldConsumer", default));
    }

    [Fact]
    public async Task Should_publish_event_on_start()
    {
        var testState = new TestState<EventConsumerState>(consumerName1, persistenceFactory)
        {
            Snapshot = new EventConsumerState
            {
                Position = "42"
            },
            Version = 0
        };

        var response = await sut.StartAsync(consumerName1, default);

        A.CallTo(() => messaging.PublishAsync(new EventConsumerStart(consumerName1), null, default))
            .MustHaveHappened();

        Assert.Equal("42", response.Position);
    }

    [Fact]
    public async Task Should_publish_event_on_stop()
    {
        var testState = new TestState<EventConsumerState>(consumerName1, persistenceFactory)
        {
            Snapshot = new EventConsumerState
            {
                Position = "42"
            },
            Version = 0
        };

        var response = await sut.StopAsync(consumerName1, default);

        A.CallTo(() => messaging.PublishAsync(new EventConsumerStop(consumerName1), null, default))
            .MustHaveHappened();

        Assert.Equal("42", response.Position);
    }

    [Fact]
    public async Task Should_publish_event_on_reset()
    {
        var testState = new TestState<EventConsumerState>(consumerName1, persistenceFactory)
        {
            Snapshot = new EventConsumerState
            {
                Position = "42"
            },
            Version = 0
        };

        var response = await sut.ResetAsync(consumerName1, default);

        A.CallTo(() => messaging.PublishAsync(new EventConsumerReset(consumerName1), null, default))
            .MustHaveHappened();

        Assert.Equal("42", response.Position);
    }
}
