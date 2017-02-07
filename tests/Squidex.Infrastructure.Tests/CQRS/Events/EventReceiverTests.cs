// ==========================================================================
//  EventReceiverTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
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

        private readonly Mock<ILiveEventConsumer> liveConsumer1 = new Mock<ILiveEventConsumer>();
        private readonly Mock<ILiveEventConsumer> liveConsumer2 = new Mock<ILiveEventConsumer>();
        private readonly Mock<ICatchEventConsumer> catchConsumer1 = new Mock<ICatchEventConsumer>();
        private readonly Mock<ICatchEventConsumer> catchConsumer2 = new Mock<ICatchEventConsumer>();
        private readonly Mock<IEventStream> eventStream = new Mock<IEventStream>();
        private readonly Mock<EventDataFormatter> formatter = new Mock<EventDataFormatter>(new TypeNameRegistry(), null);
        private readonly EventData eventData = new EventData();
        private readonly Envelope<IEvent> envelope = new Envelope<IEvent>(new MyEvent());
        private readonly MyLogger logger = new MyLogger();

        public EventReceiverTests()
        {
            formatter.Setup(x => x.Parse(eventData)).Returns(envelope);

            eventStream.Setup(x => x.Connect("squidex", It.IsAny<Action<EventData>>())).Callback(
                new Action<string, Action<EventData>>((queue, callback) =>
                {
                    callback(eventData);
                }));
        }

        [Fact]
        public void Should_only_connect_once()
        {
            var sut = CreateSut(true);

            sut.Subscribe();
            sut.Subscribe();

            eventStream.Verify(x => x.Connect("squidex", It.IsAny<Action<EventData>>()), Times.Once());
        }

        [Fact]
        public void Should_invoke_live_consumers()
        {
            var sut = CreateSut(false);

            sut.Subscribe();

            catchConsumer1.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());
            catchConsumer2.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());

            liveConsumer1.Verify(x => x.On(envelope), Times.Once());
            liveConsumer2.Verify(x => x.On(envelope), Times.Once());

            Assert.Equal(1, logger.LogCount.Count);
            Assert.Equal(1, logger.LogCount[LogLevel.Debug]);
        }

        [Fact]
        public void Should_invoke_catch_consumers()
        {
            var sut = CreateSut(true);

            sut.Subscribe();

            liveConsumer1.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());
            liveConsumer2.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());

            catchConsumer1.Verify(x => x.On(envelope), Times.Once());
            catchConsumer2.Verify(x => x.On(envelope), Times.Once());

            Assert.Equal(1, logger.LogCount.Count);
            Assert.Equal(1, logger.LogCount[LogLevel.Debug]);
        }

        [Fact]
        public void Should_log_if_parsing_event_failed()
        {
            formatter.Setup(x => x.Parse(eventData)).Throws(new InvalidOperationException());

            var sut = CreateSut(true);

            sut.Subscribe();

            catchConsumer1.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());
            catchConsumer2.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());

            liveConsumer1.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());
            liveConsumer2.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());

            Assert.Equal(1, logger.LogCount.Count);
            Assert.Equal(1, logger.LogCount[LogLevel.Error]);
        }

        [Fact]
        public void Should_log_if_handling_failed()
        {
            catchConsumer1.Setup(x => x.On(envelope)).Throws(new InvalidOperationException());

            var sut = CreateSut(true);

            sut.Subscribe();

            catchConsumer1.Verify(x => x.On(envelope), Times.Once());
            catchConsumer2.Verify(x => x.On(envelope), Times.Once());

            liveConsumer1.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());
            liveConsumer2.Verify(x => x.On(It.IsAny<Envelope<IEvent>>()), Times.Never());

            Assert.Equal(2, logger.LogCount.Count);
            Assert.Equal(1, logger.LogCount[LogLevel.Debug]);
            Assert.Equal(1, logger.LogCount[LogLevel.Error]);
        }

        private EventReceiver CreateSut(bool canCatch)
        {
            return new EventReceiver(
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
                formatter.Object, canCatch);
        }
    }
}
