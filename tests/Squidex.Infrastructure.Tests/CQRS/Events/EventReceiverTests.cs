// ==========================================================================
//  EventReceiverTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Squidex.Infrastructure.CQRS.Events
{
    public class EventReceiverTests
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

        private sealed class MyEventSubscription : IEventSubscription
        {
            private readonly IEnumerable<StoredEvent> storedEvents;
            private bool isDisposed;

            public MyEventSubscription(IEnumerable<StoredEvent> storedEvents)
            {
                this.storedEvents = storedEvents;
            }

            public Task SubscribeAsync(Func<StoredEvent, Task> onNext, Func<Exception, Task> onError)
            {
                foreach (var storedEvent in storedEvents)
                {
                    if (isDisposed)
                    {
                        break;
                    }

                    onNext(storedEvent).Wait();
                }

                return TaskHelper.Done;
            }

            public void Dispose()
            {
                isDisposed = true;
            }
        }

        private sealed class MyEventStore : IEventStore
        {
            private readonly IEnumerable<StoredEvent> storedEvents;

            public MyEventStore(IEnumerable<StoredEvent> storedEvents)
            {
                this.storedEvents = storedEvents;
            }

            public IEventSubscription CreateSubscription(string streamFilter = null, string position = null)
            {
                return new MyEventSubscription(storedEvents);
            }

            public Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName)
            {
                throw new NotSupportedException();
            }

            public Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
            {
                throw new NotSupportedException();
            }
        }

        private readonly Mock<IEventConsumerInfoRepository> eventConsumerInfoRepository = new Mock<IEventConsumerInfoRepository>();
        private readonly Mock<IEventConsumer> eventConsumer = new Mock<IEventConsumer>();
        private readonly Mock<ISemanticLog> log = new Mock<ISemanticLog>();
        private readonly Mock<EventDataFormatter> formatter = new Mock<EventDataFormatter>(new TypeNameRegistry(), null);
        private readonly EventData eventData1 = new EventData();
        private readonly EventData eventData2 = new EventData();
        private readonly EventData eventData3 = new EventData();
        private readonly Envelope<IEvent> envelope1 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> envelope2 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> envelope3 = new Envelope<IEvent>(new MyEvent());
        private readonly EventReceiver sut;
        private readonly MyEventConsumerInfo consumerInfo = new MyEventConsumerInfo();
        private readonly string consumerName;

        public EventReceiverTests()
        {
            var events = new[]
            {
                new StoredEvent("3", 3, eventData1),
                new StoredEvent("4", 4, eventData2),
                new StoredEvent("5", 5, eventData3)
            };

            consumerName = eventConsumer.Object.GetType().Name;

            var eventStore = new MyEventStore(events);

            eventConsumer.Setup(x => x.Name).Returns(consumerName);
            eventConsumerInfoRepository.Setup(x => x.FindAsync(consumerName)).Returns(Task.FromResult<IEventConsumerInfo>(consumerInfo));

            formatter.Setup(x => x.Parse(eventData1)).Returns(envelope1);
            formatter.Setup(x => x.Parse(eventData2)).Returns(envelope2);
            formatter.Setup(x => x.Parse(eventData3)).Returns(envelope3);

            sut = new EventReceiver(formatter.Object, eventStore, eventConsumerInfoRepository.Object, log.Object);
        }

        [Fact]
        public void Should_only_connect_once()
        {
            sut.Subscribe(eventConsumer.Object);
            sut.Subscribe(eventConsumer.Object);
            sut.Refresh();
            sut.Dispose();

            eventConsumerInfoRepository.Verify(x => x.CreateAsync(consumerName), Times.Once());
        }

        [Fact]
        public void Should_subscribe_to_consumer_and_handle_events()
        {
            consumerInfo.Position = "2";

            sut.Subscribe(eventConsumer.Object);
            sut.Refresh();
            sut.Dispose();

            eventConsumer.Verify(x => x.On(envelope1), Times.Once());
            eventConsumer.Verify(x => x.On(envelope2), Times.Once());
            eventConsumer.Verify(x => x.On(envelope3), Times.Once());
        }

        [Fact]
        public void Should_abort_if_handling_failed()
        {
            consumerInfo.Position = "2";

            eventConsumer.Setup(x => x.On(envelope1)).Returns(TaskHelper.True);
            eventConsumer.Setup(x => x.On(envelope2)).Throws(new InvalidOperationException());

            sut.Subscribe(eventConsumer.Object);
            sut.Refresh();
            sut.Dispose();

            eventConsumer.Verify(x => x.On(envelope1), Times.Once());
            eventConsumer.Verify(x => x.On(envelope2), Times.Once());
            eventConsumer.Verify(x => x.On(envelope3), Times.Never());

            eventConsumerInfoRepository.Verify(x => x.StopAsync(consumerName, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void Should_abort_if_serialization_failed()
        {
            consumerInfo.Position = "2";

            formatter.Setup(x => x.Parse(eventData2)).Throws(new InvalidOperationException());

            sut.Subscribe(eventConsumer.Object);
            sut.Refresh();
            sut.Dispose();

            eventConsumer.Verify(x => x.On(envelope1), Times.Once());
            eventConsumer.Verify(x => x.On(envelope2), Times.Never());
            eventConsumer.Verify(x => x.On(envelope3), Times.Never());

            eventConsumerInfoRepository.Verify(x => x.StopAsync(consumerName, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void Should_reset_if_requested()
        {
            consumerInfo.IsResetting = true;
            consumerInfo.Position = "2";

            sut.Subscribe(eventConsumer.Object);
            sut.Refresh();
            sut.Dispose();

            eventConsumer.Verify(x => x.On(envelope1), Times.Once());
            eventConsumer.Verify(x => x.On(envelope2), Times.Once());
            eventConsumer.Verify(x => x.On(envelope3), Times.Once());

            eventConsumer.Verify(x => x.ClearAsync(), Times.Once());
        }
    }
}
