// ==========================================================================
//  EventConsumerActorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events.Actors
{
    public class EventConsumerActorTests
    {
        public sealed class MyEvent : IEvent
        {
        }

        private sealed class MyEventConsumerInfo : IEventConsumerInfo
        {
            public bool IsStopped { get; set; }

            public string Name { get; set; }

            public string Error { get; set; }

            public string Position { get; set; }
        }

        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository = A.Fake<IEventConsumerInfoRepository>();
        private readonly IEventConsumer eventConsumer = A.Fake<IEventConsumer>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IActor sutActor;
        private readonly IEventSubscriber sutSubscriber;
        private readonly EventDataFormatter formatter = A.Fake<EventDataFormatter>();
        private readonly EventData eventData = new EventData();
        private readonly Envelope<IEvent> envelope = new Envelope<IEvent>(new MyEvent());
        private readonly EventConsumerActor sut;
        private readonly MyEventConsumerInfo consumerInfo = new MyEventConsumerInfo();
        private readonly string consumerName;

        public EventConsumerActorTests()
        {
            consumerInfo.Position = Guid.NewGuid().ToString();
            consumerName = eventConsumer.GetType().Name;

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(eventSubscription);

            A.CallTo(() => eventConsumer.Name).Returns(consumerName);
            A.CallTo(() => eventConsumerInfoRepository.FindAsync(consumerName)).Returns(consumerInfo);

            A.CallTo(() => formatter.Parse(eventData, true)).Returns(envelope);

            sut = new EventConsumerActor(formatter, eventStore, eventConsumerInfoRepository, log) { ReconnectWaitMs = 0 };

            sutActor = sut;
            sutSubscriber = sut;
        }

        [Fact]
        public async Task Should_not_not_subscribe_to_event_store_when_stopped_in_db()
        {
            consumerInfo.IsStopped = true;

            await OnSubscribeAsync();

            sut.Dispose();

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_not_found_in_db()
        {
            A.CallTo(() => eventConsumerInfoRepository.FindAsync(consumerName)).Returns(Task.FromResult<IEventConsumerInfo>(null));

            await OnSubscribeAsync();

            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, null, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_not_stopped_in_db()
        {
            await OnSubscribeAsync();

            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_stop_subscription_when_stopped()
        {
            await OnSubscribeAsync();

            sutActor.Tell(new StopConsumerMessage());
            sutActor.Tell(new StopConsumerMessage());

            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, true, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_reset_consumer_when_resetting()
        {
            await OnSubscribeAsync();

            sutActor.Tell(new StopConsumerMessage());
            sutActor.Tell(new ResetConsumerMessage());
            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, true, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, null, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumer.ClearAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, consumerInfo.Position))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received()
        {
            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnSubscribeAsync();
            await OnEventAsync(eventSubscription, @event);

            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, @event.EventPosition, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_not_invoke_and_update_position_when_event_is_from_another_subscription()
        {
            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnSubscribeAsync();
            await OnEventAsync(A.Fake<IEventSubscription>(), @event);

            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, @event.EventPosition, false, null))
                .MustNotHaveHappened();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_reopen_subscription_when_exception_is_retrieved()
        {
            var ex = new InvalidOperationException();

            await OnSubscribeAsync();
            await OnErrorAsync(eventSubscription, ex);

            await Task.Delay(200);

            await sut.WaitForCompletionAsync();

            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, null))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, ex.ToString()))
                .MustNotHaveHappened();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Fact]
        public async Task Should_not_make_error_handling_when_exception_is_from_another_subscription()
        {
            var ex = new InvalidOperationException();

            await OnSubscribeAsync();
            await OnErrorAsync(A.Fake<IEventSubscription>(), ex);

            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, null))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, false, ex.ToString()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_stop_if_resetting_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventConsumer.ClearAsync())
                .Throws(ex);

            await OnSubscribeAsync();

            sutActor.Tell(new ResetConsumerMessage());
            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, true, ex.ToString()))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_stop_if_handling_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(ex);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnSubscribeAsync();
            await OnEventAsync(eventSubscription, @event);

            sut.Dispose();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, true, ex.ToString()))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_stop_if_deserialization_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => formatter.Parse(eventData, true))
                .Throws(ex);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnSubscribeAsync();
            await OnEventAsync(eventSubscription, @event);

            sut.Dispose();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, true, ex.ToString()))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_start_after_stop_when_handling_failed()
        {
            var exception = new InvalidOperationException();

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(exception);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await OnSubscribeAsync();
            await OnEventAsync(eventSubscription, @event);

            sutActor.Tell(new StartConsumerMessage());
            sutActor.Tell(new StartConsumerMessage());
            sut.Dispose();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.SetAsync(consumerName, consumerInfo.Position, true, exception.ToString()))
                .MustHaveHappened(Repeated.Exactly.Once);

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

        private Task OnSubscribeAsync()
        {
            return sut.SubscribeAsync(eventConsumer);
        }
    }
}