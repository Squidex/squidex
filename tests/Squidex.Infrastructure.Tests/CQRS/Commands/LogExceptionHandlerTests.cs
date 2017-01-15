// ==========================================================================
//  LogExceptionHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class LogExceptionHandlerTests
    {
        private readonly MyLogger logger = new MyLogger();
        private readonly LogExceptionHandler sut;

        private sealed class MyLogger : ILogger<LogExceptionHandler>
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

        private sealed class MyCommand : ICommand
        {
        }

        public LogExceptionHandlerTests()
        {
            sut = new LogExceptionHandler(logger);
        }

        [Fact]
        public async Task Should_do_nothing_if_command_is_succeeded()
        {
            var context = new CommandContext(new MyCommand());

            context.Succeed();

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(0, logger.LogCount);
        }

        [Fact]
        public async Task Should_log_if_command_failed()
        {
            var context = new CommandContext(new MyCommand());

            context.Fail(new InvalidOperationException());
            
            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(1, logger.LogCount);
        }

        [Fact]
        public async Task Should_log_if_command_is_not_handled()
        {
            var context = new CommandContext(new MyCommand());

            context.Fail(new InvalidOperationException());

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(1, logger.LogCount);
        }
    }
}