// ==========================================================================
//  EventReceiverActorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events.Actors
{
    public class EventReceiverActorTests
    {
        public sealed class MyEvent : IEvent
        {
        }

        private sealed class MyEventConsumerInfo : IEventConsumerInfo
        {
            public bool IsStopped { get; set; }
            public bool IsResetting { get; set; }

            public string Name { get; set; }
            public string Error { get; set; }
            public string Position { get; set; }
        }

        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository = A.Fake<IEventConsumerInfoRepository>();
        private readonly IEventConsumer eventConsumer = A.Fake<IEventConsumer>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly EventDataFormatter formatter = A.Fake<EventDataFormatter>();
        private readonly EventData eventData = new EventData();
        private readonly Envelope<IEvent> envelope = new Envelope<IEvent>(new MyEvent());
        private readonly EventConsumerActor sut;
        private readonly MyEventConsumerInfo consumerInfo = new MyEventConsumerInfo();
        private readonly string consumerName;

        public EventReceiverActorTests()
        {
            consumerInfo.Position = Guid.NewGuid().ToString();
            consumerName = eventConsumer.GetType().Name;

            A.CallTo(() => eventStore.CreateSubscription()).Returns(eventSubscription);

            A.CallTo(() => eventConsumer.Name).Returns(consumerName);
            A.CallTo(() => eventConsumerInfoRepository.FindAsync(consumerName)).Returns(consumerInfo);

            A.CallTo(() => formatter.Parse(eventData, true)).Returns(envelope);

            sut = new EventConsumerActor(formatter, eventStore, eventConsumerInfoRepository, log);
        }

        [Fact]
        public async Task Should_subscribe_to_event_store_when_started()
        {
            await SubscribeAsync();

            await sut.StopAsync();

            A.CallTo(() => eventConsumerInfoRepository.CreateAsync(consumerName))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StartAsync(consumerName))
                .MustHaveHappened();

            A.CallTo(() => eventSubscription.SendAsync(A<SubscribeMessage>.That.Matches(s => s.Parent == sut && s.Position == consumerInfo.Position)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_stop_subscription_when_stopped()
        {
            await SubscribeAsync();

            await sut.SendAsync(new StopConsumerMessage());
            await sut.StopAsync();

            A.CallTo(() => eventConsumerInfoRepository.CreateAsync(consumerName))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StartAsync(consumerName))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StopAsync(consumerName, null))
                .MustHaveHappened();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened();

            A.CallTo(() => eventSubscription.SendAsync(A<SubscribeMessage>.That.Matches(s => s.Parent == sut && s.Position == consumerInfo.Position)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_reset_consumer_when_resetting()
        {
            await SubscribeAsync();

            await sut.SendAsync(new ResetConsumerMessage());
            await sut.StopAsync();

            A.CallTo(() => eventConsumerInfoRepository.CreateAsync(consumerName))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StartAsync(consumerName))
                .MustHaveHappened(Repeated.Exactly.Twice);

            A.CallTo(() => eventConsumerInfoRepository.SetPositionAsync(consumerName, null, true))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StopAsync(consumerName, null))
                .MustHaveHappened();

            A.CallTo(() => eventConsumer.ClearAsync())
                .MustHaveHappened();

            A.CallTo(() => eventSubscription.SendAsync(A<SubscribeMessage>.That.Matches(s => s.Parent == sut && s.Position == consumerInfo.Position)))
                .MustHaveHappened(Repeated.Exactly.Twice);

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_and_update_position_when_event_received()
        {
            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await SubscribeAsync();

            await sut.SendAsync(new ReceiveEventMessage { Event = @event, Source = eventSubscription });
            await sut.StopAsync();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.SetPositionAsync(consumerName, @event.EventPosition, false))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_and_update_position_when_event_is_from_another_subscription()
        {
            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await SubscribeAsync();

            await sut.SendAsync(new ReceiveEventMessage { Event = @event });
            await sut.StopAsync();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.SetPositionAsync(consumerName, @event.EventPosition, false))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_stop_if_resetting_failed()
        {
            var exception = new InvalidOperationException("Exception");

            A.CallTo(() => eventConsumer.ClearAsync())
                .Throws(exception);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await SubscribeAsync();

            await sut.SendAsync(new ResetConsumerMessage());
            await sut.StopAsync();

            A.CallTo(() => eventConsumerInfoRepository.StopAsync(consumerName, exception.Message))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_stop_if_handling_failed()
        {
            var exception = new InvalidOperationException("Exception");

            A.CallTo(() => eventConsumer.On(envelope))
                .Throws(exception);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await SubscribeAsync();

            await sut.SendAsync(new ReceiveEventMessage { Event = @event, Source = eventSubscription });
            await sut.StopAsync();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.SetPositionAsync(consumerName, @event.EventPosition, false))
                .MustNotHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StopAsync(consumerName, exception.Message))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_stop_if_deserialization_failed()
        {
            var exception = new InvalidOperationException("Exception");

            A.CallTo(() => formatter.Parse(eventData, true))
                .Throws(exception);

            var @event = new StoredEvent(Guid.NewGuid().ToString(), 123, eventData);

            await SubscribeAsync();

            await sut.SendAsync(new ReceiveEventMessage { Event = @event, Source = eventSubscription });
            await sut.StopAsync();

            A.CallTo(() => eventConsumer.On(envelope))
                .MustNotHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.SetPositionAsync(consumerName, @event.EventPosition, false))
                .MustNotHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StopAsync(consumerName, exception.Message))
                .MustHaveHappened();
        }

        private async Task SubscribeAsync()
        {
            await sut.SubscribeAsync(eventConsumer);

            await Task.Delay(200);
        }
    }
}