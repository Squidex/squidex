// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Orleans.Concurrency;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrainTests
    {
        public sealed class MyEventConsumerGrain : EventConsumerGrain
        {
            public MyEventConsumerGrain(
                EventConsumerFactory eventConsumerFactory,
                IGrainState<EventConsumerState> state,
                IEventStore eventStore,
                IEventDataFormatter eventDataFormatter,
                ISemanticLog log)
                : base(eventConsumerFactory, state, eventStore, eventDataFormatter, log)
            {
            }

            protected override IEventConsumerGrain GetSelf()
            {
                return this;
            }

            protected override IEventSubscription CreateSubscription(IEventStore store, IEventSubscriber subscriber, string streamFilter, string position)
            {
                return store.CreateSubscription(subscriber, streamFilter, position);
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
        private readonly EventConsumerGrain sut;
        private readonly string consumerName;
        private readonly string initialPosition = Guid.NewGuid().ToString();

        public EventConsumerGrainTests()
        {
            grainState.Value.Position = initialPosition;

            consumerName = eventConsumer.GetType().Name;

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(eventSubscription);

            A.CallTo(() => eventConsumer.Name)
                .Returns(consumerName);

            A.CallTo(() => eventConsumer.Handles(A<StoredEvent>.Ignored))
                .Returns(true);

            A.CallTo(() => formatter.Parse(eventData, null))
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

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_not_found_in_db()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_not_stopped_in_db()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_stop_subscription_when_stopped()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();
            await sut.StopAsync();
            await sut.StopAsync();

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_reset_consumer_when_resetting()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();
            await sut.StopAsync();
            await sut.ResetAsync();

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = null, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(2, Times.Exactly);

            A.CallTo(() => eventConsumer.ClearAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, grainState.Value.Position))
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, null))
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received()
        {
            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened(1, Times.Exactly);
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

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            A.CallTo(() => formatter.Parse(eventData, null))
                .Throws(new TypeNameNotFoundException());

            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

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

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

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

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_not_make_error_handling_when_exception_is_from_another_subscription()
        {
            var ex = new InvalidOperationException();

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnErrorAsync(A.Fake<IEventSubscription>(), ex);

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => grainState.WriteAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_wakeup_when_already_subscribed()
        {
            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();
            await sut.ActivateAsync();

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

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(1, Times.Exactly);
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

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public async Task Should_stop_if_deserialization_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => formatter.Parse(eventData, null))
                .Throws(ex);

            var @event = new StoredEvent("Stream", Guid.NewGuid().ToString(), 123, eventData);

            await sut.ActivateAsync(consumerName);
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(1, Times.Exactly);
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

            await sut.StopAsync();
            await sut.StartAsync();
            await sut.StartAsync();

            grainState.Value.Should().BeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened(2, Times.Exactly);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(1, Times.Exactly);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(2, Times.Exactly);
        }

        private Task OnErrorAsync(IEventSubscription subscriber, Exception ex)
        {
            return sut.OnErrorAsync(subscriber.AsImmutable(), ex.AsImmutable());
        }

        private Task OnEventAsync(IEventSubscription subscriber, StoredEvent ev)
        {
            return sut.OnEventAsync(subscriber.AsImmutable(), ev.AsImmutable());
        }
    }
}