// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Squidex.Infrastructure.Log.Adapter;
using Xunit;

namespace Squidex.Infrastructure.Log
{
    public class SemanticLogAdapterTests
    {
        private readonly List<ILogChannel> channels = new List<ILogChannel>();
        private readonly Lazy<SemanticLog> log;
        private readonly ILogChannel channel = A.Fake<ILogChannel>();
        private readonly SemanticLogLoggerProvider sut;
        private string output;

        public SemanticLog Log
        {
            get { return log.Value; }
        }

        public SemanticLogAdapterTests()
        {
            channels.Add(channel);

            A.CallTo(() => channel.Log(A<SemanticLogLevel>.Ignored, A<string>.Ignored))
                .Invokes((SemanticLogLevel level, string message) =>
                {
                    output = message;
                });

            log = new Lazy<SemanticLog>(() => new SemanticLog(channels, new List<ILogAppender>(), () => new JsonLogWriter()));

            sut = SemanticLogLoggerProvider.ForTesting(log.Value);
        }

        [Fact]
        public void Should_do_nothing_when_disposing()
        {
            sut.Dispose();
        }

        [Fact]
        public void Should_provide_null_logger_when_no_log_provided()
        {
            var provider = SemanticLogLoggerProvider.ForTesting(null);

            Assert.Same(provider.CreateLogger("test"), NullLogger.Instance);
        }

        [Fact]
        public void Should_provide_a_scope()
        {
            var logger = sut.CreateLogger("test-category");

            Assert.NotNull(logger.BeginScope(1));
        }

        [Fact]
        public void Should_log_always()
        {
            var logger = sut.CreateLogger("test-category");

            Assert.True(logger.IsEnabled(LogLevel.Critical));
            Assert.True(logger.IsEnabled((LogLevel)123));
        }

        [Fact]
        public void Should_log_message_with_event_id()
        {
            var eventId = new EventId(1000);

            var logger = sut.CreateLogger("my-category");

            logger.Log(LogLevel.Debug, eventId, 1, null, (x, e) => "my-message");

            var expected =
                MakeTestCall(w => w
                    .WriteProperty("logLevel", "Debug")
                    .WriteProperty("message", "my-message")
                    .WriteObject("eventId", e => e
                        .WriteProperty("id", 1000))
                    .WriteProperty("category", "my-category"));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_message_with_event_id_and_name()
        {
            var eventId = new EventId(1000, "my-event");

            var logger = sut.CreateLogger("my-category");

            logger.Log(LogLevel.Debug, eventId, 1, null, (x, e) => "my-message");

            var expected =
                MakeTestCall(w => w
                    .WriteProperty("logLevel", "Debug")
                    .WriteProperty("message", "my-message")
                    .WriteObject("eventId", e => e
                        .WriteProperty("id", 1000)
                        .WriteProperty("name", "my-event"))
                    .WriteProperty("category", "my-category"));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_message_with_exception()
        {
            var exception = new InvalidOperationException();

            var logger = sut.CreateLogger("my-category");

            logger.Log(LogLevel.Debug, new EventId(0), 1, exception, (x, e) => "my-message");

            var expected =
                MakeTestCall(w => w
                    .WriteProperty("logLevel", "Debug")
                    .WriteProperty("message", "my-message")
                    .WriteException(exception)
                    .WriteProperty("category", "my-category"));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_additional_values()
        {
            var exception = new InvalidOperationException();

            var logger = sut.CreateLogger("my-category");

            logger.LogDebug("My numbers are {number1} and {Number2}", 123, 456);

            var expected =
                MakeTestCall(w => w
                    .WriteProperty("logLevel", "Debug")
                    .WriteProperty("message", "My numbers are 123 and 456")
                    .WriteProperty("number1", "123")
                    .WriteProperty("number2", "456")
                    .WriteProperty("category", "my-category"));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_not_log_numbers()
        {
            var exception = new InvalidOperationException();

            var logger = sut.CreateLogger("my-category");

            logger.LogDebug("My numbers are {0} and {1}", 123, 456);

            var expected =
                MakeTestCall(w => w
                    .WriteProperty("logLevel", "Debug")
                    .WriteProperty("message", "My numbers are 123 and 456")
                    .WriteProperty("category", "my-category"));

            Assert.Equal(expected, output);
        }

        [Theory]
        [InlineData(LogLevel.None, "Debug")]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Trace, "Trace")]
        [InlineData(LogLevel.Warning, "Warning")]
        [InlineData(LogLevel.Critical, "Fatal")]
        [InlineData(LogLevel.Information, "Information")]
        public void Should_log_message(LogLevel level, string semanticLogLevel)
        {
            var logger = sut.CreateLogger("my-category");

            logger.Log(level, new EventId(0), 1, null, (x, e) => "my-message");

            var expected =
                MakeTestCall(w => w
                    .WriteProperty("logLevel", semanticLogLevel)
                    .WriteProperty("message", "my-message")
                    .WriteProperty("category", "my-category"));

            Assert.Equal(expected, output);
        }

        private static string MakeTestCall(Action<IObjectWriter> writer)
        {
            IObjectWriter sut = new JsonLogWriter();

            writer(sut);

            return sut.ToString();
        }
    }
}
