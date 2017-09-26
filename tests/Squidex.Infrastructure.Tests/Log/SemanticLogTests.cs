// ==========================================================================
//  SemanticLogTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.Log.Adapter;
using Xunit;

namespace Squidex.Infrastructure.Log
{
    public class SemanticLogTests
    {
        private readonly List<ILogAppender> appenders = new List<ILogAppender>();
        private readonly List<ILogChannel> channels = new List<ILogChannel>();
        private readonly Lazy<SemanticLog> log;
        private readonly ILogChannel channel = A.Fake<ILogChannel>();
        private string output;

        public SemanticLog Log
        {
            get { return log.Value; }
        }

        public SemanticLogTests()
        {
            channels.Add(channel);

            A.CallTo(() => channel.Log(A<SemanticLogLevel>.Ignored, A<string>.Ignored))
                .Invokes((SemanticLogLevel level, string message) =>
                {
                    output = message;
                });

            log = new Lazy<SemanticLog>(() => new SemanticLog(channels, appenders, () => new JsonLogWriter()));
        }

        [Fact]
        public void Should_log_timestamp()
        {
            var now = DateTime.UtcNow;

            appenders.Add(new TimestampLogAppender(() => now));

            Log.LogFatal(w => { /* Do Nothing */ });

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Fatal")
                    .WriteProperty("timestamp", now));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_values_with_appender()
        {
            appenders.Add(new ConstantsLogWriter(w => w.WriteProperty("logValue", 1500)));

            Log.LogFatal(m => { });

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Fatal")
                    .WriteProperty("logValue", 1500));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_application_info()
        {
            var sessionId = Guid.NewGuid();

            appenders.Add(new ApplicationInfoLogAppender(GetType(), sessionId));

            Log.LogFatal(m => { });

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Fatal")
                    .WriteObject("app", a => a
                        .WriteProperty("name", "Squidex.Infrastructure.Tests")
                        .WriteProperty("version", "1.0.0.0")
                        .WriteProperty("sessionId", sessionId.ToString())));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_trace()
        {
            Log.LogTrace(w => w.WriteProperty("logValue", 1500));

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Trace")
                    .WriteProperty("logValue", 1500));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_debug()
        {
            Log.LogDebug(w => w.WriteProperty("logValue", 1500));

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Debug")
                    .WriteProperty("logValue", 1500));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_information()
        {
            Log.LogInformation(w => w.WriteProperty("logValue", 1500));

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Information")
                    .WriteProperty("logValue", 1500));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_warning()
        {
            Log.LogWarning(w => w.WriteProperty("logValue", 1500));

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Warning")
                    .WriteProperty("logValue", 1500));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_warning_exception()
        {
            var exception = new InvalidOperationException();

            Log.LogWarning(exception);

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Warning")
                    .WriteException(exception));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_error()
        {
            Log.LogError(w => w.WriteProperty("logValue", 1500));

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Error")
                    .WriteProperty("logValue", 1500));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_error_exception()
        {
            var exception = new InvalidOperationException();

            Log.LogError(exception);

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Error")
                    .WriteException(exception));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_fatal()
        {
            Log.LogFatal(w => w.WriteProperty("logValue", 1500));

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Fatal")
                    .WriteProperty("logValue", 1500));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_with_fatal_exception()
        {
            var exception = new InvalidOperationException();

            Log.LogFatal(exception);

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Fatal")
                    .WriteException(exception));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_log_nothing_when_exception_is_null()
        {
            Log.LogFatal((Exception)null);

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Fatal"));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_measure_trace()
        {
            Log.MeasureTrace(w => w.WriteProperty("message", "My Message")).Dispose();

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Trace")
                    .WriteProperty("message", "My Message")
                    .WriteProperty("elapsedMs", 0));

            Assert.True(output.StartsWith(expected.Substring(0, 55), StringComparison.Ordinal));
        }

        [Fact]
        public void Should_measure_debug()
        {
            Log.MeasureDebug(w => w.WriteProperty("message", "My Message")).Dispose();

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Debug")
                    .WriteProperty("message", "My Message")
                    .WriteProperty("elapsedMs", 0));

            Assert.True(output.StartsWith(expected.Substring(0, 55), StringComparison.Ordinal));
        }

        [Fact]
        public void Should_measure_information()
        {
            Log.MeasureInformation(w => w.WriteProperty("message", "My Message")).Dispose();

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Information")
                    .WriteProperty("message", "My Message")
                    .WriteProperty("elapsedMs", 0));

            Assert.True(output.StartsWith(expected.Substring(0, 55), StringComparison.Ordinal));
        }

        [Fact]
        public void Should_log_with_extensions_logger()
        {
            var exception = new InvalidOperationException();

            var loggerFactory = new LoggerFactory().AddSemanticLog(Log);
            var loggerInstance = loggerFactory.CreateLogger<SemanticLogTests>();

            loggerInstance.LogCritical(new EventId(123, "EventName"), exception, "Log {0}", 123);

            var expected =
                LogTest(w => w
                    .WriteProperty("logLevel", "Fatal")
                    .WriteProperty("message", "Log 123")
                    .WriteObject("eventId", e => e
                        .WriteProperty("id", 123)
                        .WriteProperty("name", "EventName"))
                    .WriteException(exception)
                    .WriteProperty("category", "Squidex.Infrastructure.Log.SemanticLogTests"));

            Assert.Equal(expected, output);
        }

        [Fact]
        public void Should_catch_all_exceptions_from_all_channels_when_exceptions_are_thrown()
        {
            var exception1 = new InvalidOperationException();
            var exception2 = new InvalidOperationException();

            var channel1 = A.Fake<ILogChannel>();
            var channel2 = A.Fake<ILogChannel>();

            A.CallTo(() => channel1.Log(A<SemanticLogLevel>.Ignored, A<string>.Ignored)).Throws(exception1);
            A.CallTo(() => channel2.Log(A<SemanticLogLevel>.Ignored, A<string>.Ignored)).Throws(exception2);

            var sut = new SemanticLog(new[] { channel1, channel2 }, Enumerable.Empty<ILogAppender>(), () => new JsonLogWriter());

            try
            {
                sut.Log(SemanticLogLevel.Debug, w => w.WriteProperty("should", "throw"));

                Assert.False(true);
            }
            catch (AggregateException ex)
            {
                Assert.Equal(exception1, ex.InnerExceptions[0]);
                Assert.Equal(exception2, ex.InnerExceptions[1]);
            }
        }

        private static string LogTest(Action<IObjectWriter> writer)
        {
            IObjectWriter sut = new JsonLogWriter();

            writer(sut);

            return sut.ToString();
        }
    }
}
