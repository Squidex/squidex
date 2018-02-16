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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public class EventConsumerGrainTests
    {
        public sealed class MyEventConsumerGrain : EventConsumerGrain
        {
            public MyEventConsumerGrain(IStore<string> store, IEventStore eventStore, IEventDataFormatter eventDataFormatter, ISemanticLog log)
                : base(store, eventStore, eventDataFormatter, log)
            {
            }

            protected override IEventSubscription CreateSubscription(IEventStore eventStore, string streamFilter, string position)
            {
                return eventStore.CreateSubscription(this, streamFilter, position);
            }
        }

        private readonly IEventConsumer eventConsumer = A.Fake<IEventConsumer>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscriber sutSubscriber;
        private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
        private readonly IPersistence<EventConsumerState> persistence = A.Fake<IPersistence<EventConsumerState>>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IStore<string> store = A.Fake<IStore<string>>();
        private readonly IEventDataFormatter formatter = A.Fake<IEventDataFormatter>();
        private readonly EventData eventData = new EventData();
        private readonly Envelope<IEvent> envelope = new Envelope<IEvent>(new MyEvent());
        private readonly EventConsumerGrain sut;
        private readonly string consumerName;
        private readonly string initialPosition = Guid.NewGuid().ToString();
        private Func<EventConsumerState, Task> apply;
        private EventConsumerState state = new EventConsumerState();

        public EventConsumerGrainTests()
        {
            state.Position = initialPosition;

            consumerName = eventConsumer.GetType().Name;

            A.CallTo(() => store.WithSnapshots(typeof(EventConsumerGrain), consumerName, A<Func<EventConsumerState, Task>>.Ignored))
                .Invokes(new Action<Type, string, Func<EventConsumerState, Task>>((type, key, a) => apply = a))
                .Returns(persistence);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(eventSubscription);

            A.CallTo(() => eventConsumer.Name)
                .Returns(consumerName);

            A.CallTo(() => persistence.ReadAsync(EtagVersion.Any))
                .Invokes(new Action<long>(s => apply(state)));

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .Invokes(new Action<EventConsumerState>(s => state = s));

            A.CallTo(() => formatter.Parse(eventData, true)).Returns(envelope);

            sut = new MyEventConsumerGrain(store, eventStore, formatter, log);
            sutSubscriber = sut;
        }

        [Fact]
        public void Should_not_subscribe_to_event_store_when_stopped_in_db()
        {
            state = state.Stopped();

            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);
            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_subscribe_to_event_store_when_not_found_in_db()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);
            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Should_subscribe_to_event_store_when_not_stopped_in_db()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);
            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Should_stop_subscription_when_stopped()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);
            sut.Stop();
            sut.Stop();

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = null });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Should_reset_consumer_when_resetting()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);
            sut.Stop();
            sut.Reset();
            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = null, Error = null });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Twice);

            A.CallTo(() => eventConsumer.ClearAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, state.Position))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnEventAsync(eventSubscription, @event);

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            A.CallTo(() => formatter.Parse(eventData, true))
                .Throws(new TypeNameNotFoundException());

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnEventAsync(eventSubscription, @event);

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = @event.EventPosition, Error = null });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_and_update_position_when_event_is_from_another_subscription()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnEventAsync(A.Fake<IEventSubscription>(), @event);

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_stop_if_consumer_failed()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var ex = new InvalidOperationException();

            await OnErrorAsync(eventSubscription, ex);

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_not_make_error_handling_when_exception_is_from_another_subscription()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var ex = new InvalidOperationException();

            await OnErrorAsync(A.Fake<IEventSubscription>(), ex);

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_stop_if_resetting_failed()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var ex = new InvalidOperationException();

            A.CallTo(() => eventConsumer.ClearAsync())
                .Throws(ex);

            sut.Reset();
            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_stop_if_handling_failed()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var ex = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(ex);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnEventAsync(eventSubscription, @event);

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            sut.GetState().ShouldBeEquivalentTo(new EventConsumerInfo { Name = consumerName, IsStopped = true, Position = initialPosition, Error = ex.ToString() });
        }

        [Fact]
        public async Task Should_stop_if_deserialization_failed()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var ex = new InvalidOperationException();

            A.CallTo(() => formatter.Parse(eventData, true))
                .Throws(ex);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnEventAsync(eventSubscription, @event);

            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = true, Position = initialPosition, Error = ex.ToString() });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_start_after_stop_when_handling_failed()
        {
            sut.ActivateAsync(consumerName).Wait();
            sut.Activate(eventConsumer);

            var exception = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(exception);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnEventAsync(eventSubscription, @event);

            sut.Start();
            sut.Start();
            sut.Dispose();

            state.ShouldBeEquivalentTo(new EventConsumerState { IsStopped = false, Position = initialPosition, Error = null });

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<EventConsumerState>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Twice);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        private Task OnErrorAsync(IEventSubscription subscriber, Exception ex)
        {
            return sutSubscriber.OnErrorAsync(subscriber, ex);
        }

        private Task OnEventAsync(IEventSubscription subscriber, StoredEvent ev)
        {
            return sutSubscriber.OnEventAsync(subscriber, ev);
        }
    }
}