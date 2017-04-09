// ==========================================================================
//  LogExceptionHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class LogExceptionHandlerTests
    {
        private readonly MyLog log = new MyLog();
        private readonly LogExceptionHandler sut;
        private readonly ICommand command = new Mock<ICommand>().Object;

        private sealed class MyLog : ISemanticLog
        {
            public HashSet<SemanticLogLevel> LogLevels { get; } = new HashSet<SemanticLogLevel>();

            public void Log(SemanticLogLevel logLevel, Action<IObjectWriter> action)
            {
                LogLevels.Add(logLevel);
            }

            public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
            {
                throw new NotSupportedException();
            }
        }

        public LogExceptionHandlerTests()
        {
            sut = new LogExceptionHandler(log);
        }

        [Fact]
        public async Task Should_do_nothing_if_command_is_succeeded()
        {
            var context = new CommandContext(command);

            context.Succeed();

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(0, log.LogLevels.Count);
        }

        [Fact]
        public async Task Should_log_if_command_failed()
        {
            var context = new CommandContext(command);

            context.Fail(new InvalidOperationException());
            
            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(new[] { SemanticLogLevel.Error }, log.LogLevels.ToArray());
        }

        [Fact]
        public async Task Should_log_if_command_is_not_handled()
        {
            var context = new CommandContext(command);

            var isHandled = await sut.HandleAsync(context);

            Assert.False(isHandled);
            Assert.Equal(new[] { SemanticLogLevel.Fatal }, log.LogLevels.ToArray());
        }
    }
}