// ==========================================================================
//  EventReceiverTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class EventReceiverTests
    {
        public sealed class MyEvent : IEvent
        {
        }

        private sealed class MyLogger : ILogger<EventReceiver>
        {
            public Dictionary<LogLevel, int> LogCount { get; } = new Dictionary<LogLevel, int>();

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatterr)
            {
                var count = LogCount.GetOrDefault(logLevel);

                LogCount[logLevel] = count + 1;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
        
        private readonly Mock<IEventCatchConsumer> eventConsumer = new Mock<IEventCatchConsumer>();
        private readonly Mock<IEventNotifier> eventNotifier = new Mock<IEventNotifier>();
        private readonly Mock<IEventStore> eventStore = new Mock<IEventStore>();
        private readonly Mock<EventDataFormatter> formatter = new Mock<EventDataFormatter>(new TypeNameRegistry(), null);
        private readonly EventData eventData1 = new EventData();
        private readonly EventData eventData2 = new EventData();
        private readonly EventData eventData3 = new EventData();
        private readonly EventData eventData4 = new EventData();
        private readonly Envelope<IEvent> envelope1 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> envelope2 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> envelope3 = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> envelope4 = new Envelope<IEvent>(new MyEvent());
        private readonly EventReceiver sut;
        private readonly MyLogger logger = new MyLogger();
        private readonly StoredEvent[][] events;

        public EventReceiverTests()
        {
            events = new []
            {
                new []
                {
                    new StoredEvent(3, eventData1),
                    new StoredEvent(4, eventData1)
                },
                new[]
                {
                    new StoredEvent(5, eventData1),
                    new StoredEvent(6, eventData1)
                }
            };


            formatter.Setup(x => x.Parse(eventData1)).Returns(envelope1);
            formatter.Setup(x => x.Parse(eventData2)).Returns(envelope2);
            formatter.Setup(x => x.Parse(eventData3)).Returns(envelope3);
            formatter.Setup(x => x.Parse(eventData4)).Returns(envelope4);

            sut = new EventReceiver(formatter.Object, eventStore.Object, eventNotifier.Object, logger);
        }

        [Fact]
        public void Should_only_connect_once()
        {
            sut.Subscribe(eventConsumer.Object);
            sut.Subscribe(eventConsumer.Object);

            eventConsumer.Verify(x => x.GetLastHandledEventNumber(), Times.Once());
        }

        [Fact]
        public void Should_subscribe_to_consumers_and_handle_events()
        {
            eventConsumer.Setup(x => x.GetLastHandledEventNumber()).Returns(Task.FromResult(2L));
            eventConsumer.Setup(x => x.On(It.IsAny<Envelope<IEvent>>(), It.IsAny<long>())).Returns(Task.FromResult(true));

            eventStore.Setup(x => x.GetEventsAsync(2)).Returns(events[0].ToObservable());
            eventStore.Setup(x => x.GetEventsAsync(4)).Returns(events[1].ToObservable());

            eventStore.Setup(x => x.GetEventsAsync(It.Is<long>(l => l != 2 && l != 4))).Returns(Observable.Empty<StoredEvent>());

            sut.Subscribe(eventConsumer.Object, 20);

            Task.Delay(400).ContinueWith(x => sut.Dispose()).Wait();

            Assert.Equal(1, logger.LogCount.Count);
            Assert.Equal(4, logger.LogCount[LogLevel.Debug]);

            eventConsumer.Verify(x => x.On(It.IsAny<Envelope<IEvent>>(), It.IsAny<long>()), Times.Exactly(4));
        }

        [Fact]
        public void Should_abort_if_handling_failed()
        {
            eventConsumer.Setup(x => x.GetLastHandledEventNumber()).Returns(Task.FromResult(2L));
            eventConsumer.Setup(x => x.On(It.IsAny<Envelope<IEvent>>(), It.IsAny<long>())).Throws<InvalidOperationException>();

            eventStore.Setup(x => x.GetEventsAsync(2)).Returns(events[0].ToObservable());
            eventStore.Setup(x => x.GetEventsAsync(It.Is<long>(l => l != 2 && l != 4))).Returns(Observable.Empty<StoredEvent>());

            sut.Subscribe(eventConsumer.Object, 20);

            Task.Delay(400).ContinueWith(x => sut.Dispose()).Wait();

            Assert.Equal(2, logger.LogCount.Count);
            Assert.Equal(1, logger.LogCount[LogLevel.Error]);
            Assert.Equal(1, logger.LogCount[LogLevel.Critical]);

            eventConsumer.Verify(x => x.On(It.IsAny<Envelope<IEvent>>(), It.IsAny<long>()), Times.Exactly(1));
            eventStore.Verify(x => x.GetEventsAsync(It.IsAny<long>()), Times.Exactly(1));
        }

        [Fact]
        public void Should_abort_if_serialization_failed()
        {
            eventConsumer.Setup(x => x.GetLastHandledEventNumber()).Returns(Task.FromResult(2L));
            eventConsumer.Setup(x => x.On(It.IsAny<Envelope<IEvent>>(), It.IsAny<long>())).Throws<InvalidOperationException>();

            eventStore.Setup(x => x.GetEventsAsync(2)).Returns(events[0].ToObservable());
            eventStore.Setup(x => x.GetEventsAsync(It.Is<long>(l => l != 2 && l != 4))).Returns(Observable.Empty<StoredEvent>());

            sut.Subscribe(eventConsumer.Object, 20);

            Task.Delay(400).ContinueWith(x => sut.Dispose()).Wait();

            Assert.Equal(2, logger.LogCount.Count);
            Assert.Equal(1, logger.LogCount[LogLevel.Error]);
            Assert.Equal(1, logger.LogCount[LogLevel.Critical]);

            eventConsumer.Verify(x => x.On(It.IsAny<Envelope<IEvent>>(), It.IsAny<long>()), Times.Exactly(1));
            eventStore.Verify(x => x.GetEventsAsync(It.IsAny<long>()), Times.Exactly(1));
        }
    }
}
