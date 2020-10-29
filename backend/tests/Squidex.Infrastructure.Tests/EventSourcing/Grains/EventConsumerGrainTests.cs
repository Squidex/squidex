﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrainTests
    {
        public sealed class MyEventConsumerGrain : EventConsumerGrain
        {
            private IEventSubscriber currentSubscriber;

            public MyEventConsumerGrain(
                EventConsumerFactory eventConsumerFactory,
                IGrainState<EventConsumerState> state,
                IEventStore eventStore,
                IEventDataFormatter eventDataFormatter,
                ISemanticLog log)
                : base(eventConsumerFactory, state, eventStore, eventDataFormatter, log)
            {
            }

            public Task OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent)
            {
                return currentSubscriber.OnEventAsync(subscription, storedEvent);
            }

            public Task OnErrorAsync(IEventSubscription subscription, Exception exception)
            {
                return currentSubscriber.OnErrorAsync(subscription, exception);
            }

            protected override IEventSubscription CreateRetrySubscription(IEventSubscriber subscriber)
            {
                return CreateSubscription(subscriber);
            }

            protected override IEventSubscription CreateSubscription(IEventSubscriber subscriber)
            {
                currentSubscriber = subscriber;

                return base.CreateSubscription(subscriber);
            }
        }

        private readonly IGrainState<EventConsumerState> grainState = A.Fake<IGrainState<EventConsumerState>>();
        private readonly IEventConsumer eventConsumer = A.Fake<IEventConsumer>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IEventDataFormatter formatter = A.Fake<IEventDataFormatter>();
        private readonly EventData eventData = new EventData("Type", new EnvelopeHeaders(), "Payload");
        private readonly Envelope<IEvent> envelope = new Envelope<IEvent>(new MyEvent());
        private readonly MyEventConsumerGrain sut;
        private readonly string consumerName;
        private readonly string initialPosition = Guid.NewGuid().ToString();

        public EventConsumerGrainTests()
        {
            grainState.Value.Position = initialPosition;

            consumerName = eventConsumer.GetType().Name;

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .Returns(eventSubscription);

            A.CallTo(() => eventConsumer.Name)
                .Returns(consumerName);

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

            A.CallTo(() => eventSubscription.Sender)
                .Returns(eventSubscription);

            A.CallTo(() => formatter.ParseIfKnown(A<StoredEvent>.That.Matches(x => x.Data == eventData)))
                .Returns(envelope);

            sut = new MyEventConsumerGrain(
                x => eventConsumer,
                grainState,
                eventStore,
                formatter,
                log);
        }

        [Fact]
        public async Task Should_not_subscribe_to_event_store_when_stopped_in_db()
        {
            grainState.Value = grainState.Value.Stopped();

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_not_found_in_db()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_failed()
        {
            grainState.Value = grainState.Value.Stopped(new InvalidOperationException());

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_not_stopped_in_db()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_stop_subscription_when_stopped()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();
            await sut.StopAsync();
            await sut.StopAsync();

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_reset_consumer_when_resetting()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();
            await sut.StopAsync();
            await sut.ResetAsync();

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = null, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(2, Times.Exactly);

            A.CallTo(() => eventConsumer.ClearAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, grainState.Value.Position))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, null))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received()
        {
            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null, Count = 1 });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received_one_by_one()
        {
            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            A.CallTo(() => eventConsumer.BatchSize)
                .Returns(1);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null, Count = 5 });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(5, Times.Exactly);

            A.CallTo(() => eventConsumer.On(A<IEnumerable<Envelope<IEvent>>>._))
                .MustHaveHappened(5, Times.Exactly);
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received_batched()
        {
            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            A.CallTo(() => eventConsumer.BatchSize)
                .Returns(100);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);
            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null, Count = 5 });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventConsumer.On(A<IEnumerable<Envelope<IEvent>>>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_invoke_but_update_position_when_consumer_does_not_want_to_handle()
        {
            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            A.CallTo(() => eventConsumer.Handles(@event))
                .Returns(false);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null, Count = 0 });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            A.CallTo(() => formatter.ParseIfKnown(A<StoredEvent>.That.Matches(x => x.Data == eventData)))
                .Returns(null);

            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null, Count = 0 });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_and_update_position_when_event_is_from_another_subscription()
        {
            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(A.Fake<IEventSubscription>(), @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_stop_if_consumer_failed()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            var ex = new InvalidOperationException();

            await OnErrorAsync(eventSubscription, ex);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_make_error_handling_when_exception_is_from_another_subscription()
        {
            var ex = new InvalidOperationException();

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnErrorAsync(A.Fake<IEventSubscription>(), ex);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_wakeup_when_already_subscribed()
        {
            await sut.ActivateAsync(consumerName);
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

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();
            await sut.ResetAsync();

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_stop_if_handling_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(ex);

            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_stop_if_deserialization_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => formatter.ParseIfKnown(A<StoredEvent>.That.Matches(x => x.Data == eventData)))
                .Throws(ex);

            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            AssetGrainState(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_start_after_stop_when_handling_failed()
        {
            var exception = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(exception);

            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            await sut.CompleteAsync();

            await sut.StopAsync();
            await sut.StartAsync();
            await sut.StartAsync();

            AssetGrainState(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(2, Times.Exactly);

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .MustHaveHappened(2, Times.Exactly);
        }

        private Task OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return sut.OnErrorAsync(subscription, exception);
        }

        private Task OnEventAsync(IEventSubscription subscription, StoredEvent ev)
        {
            return sut.OnEventAsync(subscription, ev);
        }

        private void AssetGrainState(EventConsumerState state)
        {
            grainState.Value.Should().BeEquivalentTo(state);
        }
    }
}