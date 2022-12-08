// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing.Consume;

public class EventConsumerProcessorTests
{
    public sealed class MyEventConsumerProcessor : EventConsumerProcessor
    {
        public IEventSubscriber<StoredEvent>? Subscriber { get; set; }

        public MyEventConsumerProcessor(
            IPersistenceFactory<EventConsumerState> persistenceFactory,
            IEventConsumer eventConsumer,
            IEventFormatter eventFormatter,
            IEventStore eventStore,
            ILogger<EventConsumerProcessor> log)
            : base(persistenceFactory, eventConsumer, eventFormatter, eventStore, log)
        {
        }

        protected override IEventSubscription CreateRetrySubscription(IEventSubscriber<ParsedEvents> subscriber)
        {
            return CreatePipeline(subscriber);
        }

        protected override IEventSubscription CreateSubscription(IEventSubscriber<StoredEvent> subscriber)
        {
            Subscriber = subscriber;

            return base.CreateSubscription(subscriber);
        }
    }

    private readonly IEventConsumer eventConsumer = A.Fake<IEventConsumer>();
    private readonly IEventFormatter eventFormatter = A.Fake<IEventFormatter>();
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
    private readonly TestState<EventConsumerState> state;
    private readonly StoredEvent storedEvent;
    private readonly EventData eventData = new EventData("Type", new EnvelopeHeaders(), "Payload");
    private readonly Envelope<IEvent> envelope = new Envelope<IEvent>(new MyEvent());
    private readonly MyEventConsumerProcessor sut;
    private readonly string consumerName = Guid.NewGuid().ToString();
    private readonly string initialPosition = Guid.NewGuid().ToString();

    public EventConsumerProcessorTests()
    {
        state = new TestState<EventConsumerState>(DomainId.Create(consumerName))
        {
            Snapshot = new EventConsumerState
            {
                Position = initialPosition
            }
        };

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .Returns(eventSubscription);

        A.CallTo(() => eventConsumer.Name)
            .Returns(consumerName);

        A.CallTo(() => eventConsumer.CanClear)
            .Returns(true);

        A.CallTo(() => eventConsumer.Handles(A<StoredEvent>._))
            .Returns(true);

        A.CallTo(() => eventConsumer.On(A<IEnumerable<Envelope<IEvent>>>._))
            .Invokes((IEnumerable<Envelope<IEvent>> events) =>
            {
                foreach (var @event in events)
                {
                    eventConsumer.On(@event).Wait();
                }
            });

        storedEvent = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

        A.CallTo(() => eventFormatter.ParseIfKnown(storedEvent))
            .Returns(envelope);

        sut = new MyEventConsumerProcessor(
            state.PersistenceFactory,
            eventConsumer,
            eventFormatter,
            eventStore,
            A.Fake<ILogger<EventConsumerProcessor>>());
    }

    [Fact]
    public async Task Should_query_position_if_consumer_should_start_from_latest()
    {
        state.Snapshot = new EventConsumerState();

        A.CallTo(() => eventConsumer.StartLatest)
            .Returns(true);

        A.CallTo(() => eventConsumer.EventsFilter)
            .Returns("my-filter");

        var latestPosition = "LATEST";

        A.CallTo(() => eventStore.QueryAllReverseAsync("my-filter", default, 1, A<CancellationToken>._))
            .Returns(Enumerable.Repeat(new StoredEvent("Stream", latestPosition, 1, eventData), 1).ToAsyncEnumerable());

        await sut.InitializeAsync(default);

        AssertGrainState(isStopped: false, position: latestPosition);
    }

    [Fact]
    public async Task Should_not_subscribe_to_event_store_if_stopped_in_db()
    {
        state.Snapshot = state.Snapshot.Stopped();

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: initialPosition);

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_subscribe_to_event_store_if_not_found_in_db()
    {
        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: initialPosition);

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_subscribe_to_event_store_if_failed()
    {
        state.Snapshot = state.Snapshot.Stopped(new InvalidOperationException());

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: initialPosition);

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_subscribe_to_event_store_if_not_stopped_in_db()
    {
        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: initialPosition);

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_stop_subscription_if_stopped()
    {
        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.StopAsync();
        await sut.StopAsync();

        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: initialPosition);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_reset_consumer_if_resetting()
    {
        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.StopAsync();
        await sut.ResetAsync();

        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: null);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappened(2, Times.Exactly);

        A.CallTo(() => eventConsumer.ClearAsync())
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, state.Snapshot.Position))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, null))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_reset_consumer_if_not_allowed()
    {
        A.CallTo(() => eventConsumer.CanClear)
            .Returns(false);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.StopAsync();
        await sut.ResetAsync();

        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: initialPosition);

        A.CallTo(() => eventConsumer.ClearAsync())
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_and_update_position_if_event_received()
    {
        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: storedEvent.EventPosition, count: 1);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventConsumer.On(envelope))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_invoke_and_update_position_if_event_received_one_by_one()
    {
        A.CallTo(() => eventConsumer.BatchSize)
            .Returns(1);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);

        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: storedEvent.EventPosition, count: 5);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappened(5, Times.Exactly);

        A.CallTo(() => eventConsumer.On(A<IEnumerable<Envelope<IEvent>>>._))
            .MustHaveHappened(5, Times.Exactly);
    }

    [Fact]
    public async Task Should_invoke_and_update_position_if_events_received_batched()
    {
        A.CallTo(() => eventConsumer.BatchSize)
            .Returns(5);

        A.CallTo(() => eventConsumer.BatchDelay)
            .Returns(int.MaxValue);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);
        await OnNextAsync(eventSubscription, storedEvent);

        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: storedEvent.EventPosition, count: 5);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventConsumer.On(A<IEnumerable<Envelope<IEvent>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_invoke_but_update_position_if_consumer_does_not_want_to_handle()
    {
        A.CallTo(() => eventConsumer.Handles(storedEvent))
            .Returns(false);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: storedEvent.EventPosition);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventConsumer.On(envelope))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_old_events()
    {
        A.CallTo(() => eventFormatter.ParseIfKnown(A<StoredEvent>.That.Matches(x => x.Data == eventData)))
            .Returns(null);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: storedEvent.EventPosition);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventConsumer.On(envelope))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_and_update_position_if_event_is_from_another_subscription()
    {
        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.OnNextAsync(A.Fake<IEventSubscription>(), new ParsedEvents(new[] { envelope }.ToList(), storedEvent.EventPosition));
        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: initialPosition);

        A.CallTo(() => eventConsumer.On(envelope))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_stop_if_consumer_failed()
    {
        var ex = new InvalidOperationException();

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnErrorAsync(eventSubscription, ex);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: initialPosition, error: ex.Message);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_make_error_handling_if_exception_is_from_another_subscription()
    {
        var ex = new InvalidOperationException();

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.OnErrorAsync(A.Fake<IEventSubscription>(), ex);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: false, position: initialPosition);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_wakeup_if_already_subscribed()
    {
        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.ActivateAsync();
        await sut.CompleteAsync();

        A.CallTo(() => eventSubscription.WakeUp())
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_stop_if_resetting_failed()
    {
        var ex = new InvalidOperationException();

        A.CallTo(() => eventConsumer.ClearAsync())
            .Throws(ex);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await sut.ResetAsync();
        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: initialPosition, error: ex.Message);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_stop_if_handling_failed()
    {
        var ex = new InvalidOperationException();

        A.CallTo(() => eventConsumer.On(envelope))
            .Throws(ex);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: initialPosition, error: ex.Message);

        A.CallTo(() => eventConsumer.On(envelope))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_stop_if_deserialization_failed()
    {
        var ex = new InvalidOperationException();

        A.CallTo(() => eventFormatter.ParseIfKnown(A<StoredEvent>.That.Matches(x => x.Data == eventData)))
            .Throws(ex);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: initialPosition, error: ex.Message);

        A.CallTo(() => eventConsumer.On(envelope))
            .MustNotHaveHappened();

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_start_after_stop_if_handling_failed()
    {
        var ex = new InvalidOperationException();

        A.CallTo(() => eventConsumer.On(envelope))
            .Throws(ex);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await sut.CompleteAsync();

        await sut.StopAsync();
        await sut.StartAsync();
        await sut.StartAsync();

        AssertGrainState(isStopped: false, position: initialPosition);

        A.CallTo(() => eventConsumer.On(envelope))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .MustHaveHappened(2, Times.Exactly);

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public async Task Should_fail_if_writing_failed()
    {
        var ex = new InconsistentStateException(0, 1);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<EventConsumerState>._, A<CancellationToken>._))
            .Throws(ex);

        await sut.InitializeAsync(default);
        await sut.ActivateAsync();

        await OnNextAsync(eventSubscription, storedEvent);
        await sut.CompleteAsync();

        AssertGrainState(isStopped: true, position: storedEvent.EventPosition, error: ex.Message, 1);
    }

    private ValueTask OnErrorAsync(IEventSubscription subscription, Exception exception)
    {
        return sut.Subscriber?.OnErrorAsync(subscription, exception) ?? default;
    }

    private ValueTask OnNextAsync(IEventSubscription subscription, StoredEvent @event)
    {
        return sut.Subscriber?.OnNextAsync(subscription, @event) ?? default;
    }

    private void AssertGrainState(bool isStopped = false, string? position = null, string? error = null, int count = 0)
    {
        sut.State.Should().BeEquivalentTo(
            new EventConsumerState { IsStopped = isStopped, Position = position, Error = error, Count = count });
    }
}
