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
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Xunit;

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

            public async Task SubscribeAsync(Func<StoredEvent, Task> onNext, Func<Exception, Task> onError)
            {
                foreach (var storedEvent in storedEvents)
                {
                    if (isDisposed)
                    {
                        break;
                    }

                    try
                    {
                        await onNext(storedEvent);
                    }
                    catch (Exception ex)
                    {
                        await onError(ex);
                    }
                }
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

            public Task AppendEventsAsync(Guid commitId, string streamName, ICollection<EventData> events)
            {
                throw new NotSupportedException();
            }

            public Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
            {
                throw new NotSupportedException();
            }
        }

        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository = A.Fake<IEventConsumerInfoRepository>();
        private readonly IEventConsumer eventConsumer = A.Fake<IEventConsumer>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly EventDataFormatter formatter = A.Fake<EventDataFormatter>();
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

            consumerName = eventConsumer.GetType().Name;

            var eventStore = new MyEventStore(events);

            A.CallTo(() => eventConsumer.Name).Returns(consumerName);
            A.CallTo(() => eventConsumerInfoRepository.FindAsync(consumerName)).Returns(consumerInfo);

            A.CallTo(() => formatter.Parse(eventData1)).Returns(envelope1);
            A.CallTo(() => formatter.Parse(eventData2)).Returns(envelope2);
            A.CallTo(() => formatter.Parse(eventData3)).Returns(envelope3);

            sut = new EventReceiver(formatter, eventStore, eventConsumerInfoRepository, log);
        }

        [Fact]
        public void Should_only_connect_once()
        {
            sut.Subscribe(eventConsumer);
            sut.Subscribe(eventConsumer);
            sut.Refresh();
            sut.Dispose();

            A.CallTo(() => eventConsumerInfoRepository.CreateAsync(consumerName)).MustHaveHappened();
        }

        [Fact]
        public void Should_subscribe_to_consumer_and_handle_events()
        {
            consumerInfo.Position = "2";

            sut.Subscribe(eventConsumer);
            sut.Refresh();
            sut.Dispose();

            A.CallTo(() => eventConsumer.On(envelope1)).MustHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope2)).MustHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope3)).MustHaveHappened();
        }

        [Fact]
        public void Should_abort_if_handling_failed()
        {
            consumerInfo.Position = "2";

            A.CallTo(() => eventConsumer.On(envelope1)).Returns(TaskHelper.True);
            A.CallTo(() => eventConsumer.On(envelope2)).Throws(new InvalidOperationException());

            sut.Subscribe(eventConsumer);
            sut.Refresh();
            sut.Dispose();

            A.CallTo(() => eventConsumer.On(envelope1)).MustHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope2)).MustHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope3)).MustNotHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StopAsync(consumerName, A<string>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void Should_abort_if_serialization_failed()
        {
            consumerInfo.Position = "2";

            A.CallTo(() => formatter.Parse(eventData2)).Throws(new InvalidOperationException());

            sut.Subscribe(eventConsumer);
            sut.Refresh();
            sut.Dispose();

            A.CallTo(() => eventConsumer.On(envelope1)).MustHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope2)).MustNotHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope3)).MustNotHaveHappened();

            A.CallTo(() => eventConsumerInfoRepository.StopAsync(consumerName, A<string>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public void Should_reset_if_requested()
        {
            consumerInfo.IsResetting = true;
            consumerInfo.Position = "2";

            sut.Subscribe(eventConsumer);
            sut.Refresh();
            sut.Dispose();

            A.CallTo(() => eventConsumer.On(envelope1)).MustHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope2)).MustHaveHappened();
            A.CallTo(() => eventConsumer.On(envelope3)).MustHaveHappened();

            A.CallTo(() => eventConsumer.ClearAsync()).MustHaveHappened();
        }
    }
}
