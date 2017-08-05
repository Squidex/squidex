// ==========================================================================
//  LogExecutingHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class LogExecutingHandlerTests
    {
        private readonly MyLog log = new MyLog();
        private readonly LogExecutingHandler sut;
        private readonly ICommand command = A.Dummy<ICommand>();

        private sealed class MyLog : ISemanticLog
        {
            public int LogCount { get; private set; }

            public void Log(SemanticLogLevel logLevel, Action<IObjectWriter> action)
            {
                LogCount++;
            }

            public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
            {
                throw new NotSupportedException();
            }
        }

        public LogExecutingHandlerTests()
        {
            sut = new LogExecutingHandler(log);
        }

        [Fact]
        public async Task Should_log_once()
        {
            var context = new CommandContext(command);

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(1, log.LogCount);
        }
    }
}
