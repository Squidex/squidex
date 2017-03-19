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
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class LogExceptionHandlerTests
    {
        private readonly MyLogger logger = new MyLogger();
        private readonly LogExceptionHandler sut;
        private readonly ICommand command = new Mock<ICommand>().Object;

        private sealed class MyLogger : ILogger<LogExceptionHandler>
        {
            public HashSet<LogLevel> LogLevels { get; } = new HashSet<LogLevel>();

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatterr)
            {
                LogLevels.Add(logLevel);
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

        public LogExceptionHandlerTests()
        {
            sut = new LogExceptionHandler(logger);
        }

        [Fact]
        public async Task Should_do_nothing_if_command_is_succeeded()
        {
            var context = new CommandContext(command);

            context.Succeed();

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(0, logger.LogLevels.Count);
        }

        [Fact]
        public async Task Should_log_if_command_failed()
        {
            var context = new CommandContext(command);

            context.Fail(new InvalidOperationException());
            
            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(new[] { LogLevel.Error }, logger.LogLevels.ToArray());
        }

        [Fact]
        public async Task Should_log_if_command_is_not_handled()
        {
            var context = new CommandContext(command);

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(new[] { LogLevel.Critical }, logger.LogLevels.ToArray());
        }
    }
}