// ==========================================================================
//  EventConsumerGrainTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Infrastructure.CQRS.Events.Orleans.Grains.Implementation;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events.Grains
{
    public class EventConsumerGrainTests
    {
        public sealed class MyEvent : IEvent
        {
        }

        public sealed class MyEventConsumerActor : EventConsumerGrain
        {
            public MyEventConsumerActor(
                EventDataFormatter formatter,
                EventConsumerFactory eventConsumerFactory,
                IEventStore eventStore,
                ISemanticLog log,
                IGrainIdentity identity,
                IGrainRuntime runtime,
                IStorage<EventConsumerGrainState> storage)
                : base(formatter, eventConsumerFactory, eventStore, log, identity, runtime, storage)
            {
            }

            protected override IEventSubscription CreateSubscription(IEventStore eventStore, string streamFilter, string position)
            {
                return eventStore.CreateSubscription(this, streamFilter, position);
            }
        }

        private readonly IEventConsumer eventConsumer = A.Fake<IEventConsumer>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IEventSubscriber sutSubscriber;
        private readonly IStorage<EventConsumerGrainState> storage = A.Fake<IStorage<EventConsumerGrainState>>();
        private readonly EventDataFormatter formatter = A.Fake<EventDataFormatter>();
        private readonly EventData eventData = new EventData();
        private readonly Envelope<IEvent> envelope = new Envelope<IEvent>(new MyEvent());
        private readonly EventConsumerFactory factory;
        private readonly MyEventConsumerActor sut;
        private readonly string consumerName;
        private EventConsumerGrainState state = new EventConsumerGrainState();

        public EventConsumerGrainTests()
        {
            factory = x => eventConsumer;

            state.Position = Guid.NewGuid().ToString();
            consumerName = eventConsumer.GetType().Name;

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(eventSubscription);
            A.CallTo(() => eventConsumer.Name).Returns(consumerName);

            A.CallTo(() => formatter.Parse(eventData, true)).Returns(envelope);

            A.CallTo(() => storage.State).ReturnsLazily(() => state);
            A.CallToSet(() => storage.State).Invokes(new Action<EventConsumerGrainState>(s => state = s));

            sut = new MyEventConsumerActor(
                formatter,
                factory,
                eventStore,
                log,
                A.Fake<IGrainIdentity>(),
                A.Fake<IGrainRuntime>(),
                storage);

            sutSubscriber = sut;
        }

        [Fact]
        public async Task Should_not_subscribe_to_event_store_when_stopped_in_db()
        {
            state.IsStopped = true;

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_not_stopped_in_db()
        {
            state.Position = "123";

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, "123"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_stop_subscription_when_stopped()
        {
            await sut.OnActivateAsync();
            await sut.ActivateAsync();
            await sut.StopAsync();
            await sut.StopAsync();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.True(state.IsStopped);
        }

        [Fact]
        public async Task Should_reset_consumer_when_resetting()
        {
            await sut.OnActivateAsync();
            await sut.ActivateAsync();
            await sut.StopAsync();
            await sut.ResetAsync();

            A.CallTo(() => eventConsumer.ClearAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, state.Position))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.False(state.IsStopped);
        }

        [Fact]
        public async Task Should_unsubscribe_from_subscription_when_closed()
        {
            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnClosedAsync(eventSubscription);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened();

            Assert.False(state.IsStopped);
        }

        [Fact]
        public async Task Should_not_unsubscribe_from_subscription_when_closed_call_is_from_another_subscription()
        {
            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnClosedAsync(A.Fake<IEventSubscription>());

            A.CallTo(() => eventSubscription.StopAsync())
                .MustNotHaveHappened();

            Assert.False(state.IsStopped);
        }

        [Fact]
        public async Task Should_not_unsubscribe_from_subscription_when_not_running()
        {
            state.IsStopped = true;

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnClosedAsync(A.Fake<IEventSubscription>());

            A.CallTo(() => storage.WriteStateAsync())
                .MustNotHaveHappened();

            Assert.True(state.IsStopped);
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received()
        {
            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(@event.EventPosition, state.Position);

            var info = await sut.GetStateAsync();

            Assert.Equal(@event.EventPosition, info.Value.Position);
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            A.CallTo(() => formatter.Parse(eventData, true))
                .Throws(new TypeNameNotFoundException());

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            Assert.Equal(@event.EventPosition, state.Position);
        }

        [Fact]
        public async Task Should_not_invoke_and_update_position_when_event_is_from_another_subscription()
        {
            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnEventAsync(A.Fake<IEventSubscription>(), @event);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_make_error_handling_when_exception_is_from_another_subscription()
        {
            var ex = new InvalidOperationException();

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnErrorAsync(A.Fake<IEventSubscription>(), ex);

            Assert.False(state.IsStopped);
        }

        [Fact]
        public async Task Should_stop_if_subscription_failed()
        {
            var ex = new InvalidOperationException();

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnErrorAsync(eventSubscription, ex);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.True(state.IsStopped);
        }

        [Fact]
        public async Task Should_stop_if_subscription_failed_and_ignore_error_on_unsubscribe()
        {
            A.CallTo(() => eventSubscription.StopAsync())
                .Throws(new InvalidOperationException());

            var ex = new InvalidOperationException();

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnErrorAsync(eventSubscription, ex);

            Assert.True(state.IsStopped);
        }

        [Fact]
        public async Task Should_stop_if_resetting_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventConsumer.ClearAsync())
                .Throws(ex);

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await sut.ResetAsync();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.True(state.IsStopped);
        }

        [Fact]
        public async Task Should_stop_if_handling_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(ex);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.True(state.IsStopped);
        }

        [Fact]
        public async Task Should_stop_if_deserialization_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => formatter.Parse(eventData, true))
                .Throws(ex);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.True(state.IsStopped);
        }

        [Fact]
        public async Task Should_start_after_stop_when_handling_failed()
        {
            var exception = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(exception);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await sut.OnActivateAsync();
            await sut.ActivateAsync();

            await OnEventAsync(eventSubscription, @event);

            Assert.True(state.IsStopped);

            await sut.StartAsync();
            await sut.StartAsync();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Twice);

            Assert.False(state.IsStopped);
        }

        private Task OnErrorAsync(IEventSubscription subscriber, Exception ex)
        {
            return sutSubscriber.OnErrorAsync(subscriber, ex);
        }

        private Task OnEventAsync(IEventSubscription subscriber, StoredEvent ev)
        {
            return sutSubscriber.OnEventAsync(subscriber, ev);
        }

        private Task OnClosedAsync(IEventSubscription subscriber)
        {
            return sutSubscriber.OnClosedAsync(subscriber);
        }
    }
}