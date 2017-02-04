// ==========================================================================
//  EventReceiverTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
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
            public int LogCount { get; private set; }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatterr)
            {
                LogCount++;
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

        private readonly Mock<ILiveEventConsumer> liveConsumer1 = new Mock<ILiveEventConsumer>();
        private readonly Mock<ILiveEventConsumer> liveConsumer2 = new Mock<ILiveEventConsumer>();
        private readonly Mock<ICatchEventConsumer> catchConsumer1 = new Mock<ICatchEventConsumer>();
        private readonly Mock<ICatchEventConsumer> catchConsumer2 = new Mock<ICatchEventConsumer>();
        private readonly Mock<IEventStream> eventStream = new Mock<IEventStream>();
        private readonly Mock<EventDataFormatter> formatter = new Mock<EventDataFormatter>(new TypeNameRegistry(), null);
        private readonly EventData eventDataPast = new EventData();
        private readonly EventData eventDataFuture = new EventData();
        private readonly Envelope<IEvent> eventPast = new Envelope<IEvent>(new MyEvent());
        private readonly Envelope<IEvent> eventFuture = new Envelope<IEvent>(new MyEvent());
        private readonly MyLogger logger = new MyLogger();
        private readonly EventReceiver sut;

        public EventReceiverTests()
        {
            eventFuture.SetTimestamp(Instant.FromDateTimeUtc(DateTime.UtcNow.AddYears(1)));

            formatter.Setup(x => x.Parse(eventDataPast)).Returns(eventPast);
            formatter.Setup(x => x.Parse(eventDataFuture)).Returns(eventFuture);

            eventStream.Setup(x => x.Connect("squidex", It.IsAny<Action<EventData>>())).Callback(
                new Action<string, Action<EventData>>((queue, callback) =>
                {
                    callback(eventDataPast);
                    callback(eventDataFuture);
                }));

            sut =
                new EventReceiver(
                    logger, 
                    eventStream.Object,
                    new[]
                    {
                        liveConsumer1.Object,
                        liveConsumer2.Object
                    },
                    new[]
                    {
                        catchConsumer1.Object,
                        catchConsumer2.Object
                    },
                    formatter.Object);
        }

        [Fact]
        public void Should_only_connect_once()
        {
            sut.Subscribe();
            sut.Subscribe();

            eventStream.Verify(x => x.Connect("squidex", It.IsAny<Action<EventData>>()), Times.Once());
        }

        [Fact]
        public void Should_invoke_consumers()
        {
            sut.Subscribe();

            catchConsumer1.Verify(x => x.On(eventPast), Times.Once());
            catchConsumer2.Verify(x => x.On(eventPast), Times.Once());

            catchConsumer1.Verify(x => x.On(eventFuture), Times.Once());
            catchConsumer2.Verify(x => x.On(eventFuture), Times.Once());

            liveConsumer1.Verify(x => x.On(eventPast), Times.Never());
            liveConsumer2.Verify(x => x.On(eventPast), Times.Never());

            liveConsumer1.Verify(x => x.On(eventFuture), Times.Once());
            liveConsumer2.Verify(x => x.On(eventFuture), Times.Once());

            Assert.Equal(0, logger.LogCount);
        }
        
        [Fact]
        public void Should_log_if_parsing_event_failed()
        {
            formatter.Setup(x => x.Parse(eventDataPast)).Throws(new InvalidOperationException());

            sut.Subscribe();

            catchConsumer1.Verify(x => x.On(eventPast), Times.Never());
            catchConsumer2.Verify(x => x.On(eventPast), Times.Never());

            catchConsumer1.Verify(x => x.On(eventFuture), Times.Once());
            catchConsumer2.Verify(x => x.On(eventFuture), Times.Once());

            liveConsumer1.Verify(x => x.On(eventPast), Times.Never());
            liveConsumer2.Verify(x => x.On(eventPast), Times.Never());

            liveConsumer1.Verify(x => x.On(eventFuture), Times.Once());
            liveConsumer2.Verify(x => x.On(eventFuture), Times.Once());

            Assert.Equal(1, logger.LogCount);
        }

        [Fact]
        public void Should_log_if_handling_failed()
        {
            catchConsumer1.Setup(x => x.On(eventPast)).Throws(new InvalidOperationException());

            liveConsumer1.Setup(x => x.On(eventFuture)).Throws(new InvalidOperationException());

            sut.Subscribe();

            catchConsumer1.Verify(x => x.On(eventPast), Times.Once());
            catchConsumer2.Verify(x => x.On(eventPast), Times.Once());

            catchConsumer1.Verify(x => x.On(eventFuture), Times.Once());
            catchConsumer2.Verify(x => x.On(eventFuture), Times.Once());

            liveConsumer1.Verify(x => x.On(eventPast), Times.Never());
            liveConsumer2.Verify(x => x.On(eventPast), Times.Never());

            liveConsumer1.Verify(x => x.On(eventFuture), Times.Once());
            liveConsumer2.Verify(x => x.On(eventFuture), Times.Once());

            Assert.Equal(2, logger.LogCount);
        }
    }
}
