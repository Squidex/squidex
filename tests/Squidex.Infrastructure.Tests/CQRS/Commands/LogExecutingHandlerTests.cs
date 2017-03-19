// ==========================================================================
//  LogExecutingHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class LogExecutingHandlerTests
    {
        private readonly MyLogger logger = new MyLogger();
        private readonly LogExecutingHandler sut;
        private readonly ICommand command = new Mock<ICommand>().Object;

        private sealed class MyLogger : ILogger<LogExecutingHandler>
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
        
        public LogExecutingHandlerTests()
        {
            sut = new LogExecutingHandler(logger);
        }

        [Fact]
        public async Task Should_log_once()
        {
            var context = new CommandContext(command);

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(1, logger.LogCount);
        }
    }
}
