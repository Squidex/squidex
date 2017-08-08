// ==========================================================================
//  LogExceptionHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class LogExceptionHandlerTests
    {
        private readonly MyLog log = new MyLog();
        private readonly LogCommandHandler sut;
        private readonly ICommand command = A.Dummy<ICommand>();

        private sealed class MyLog : ISemanticLog
        {
            public int LogCount { get; private set; }

            public Dictionary<SemanticLogLevel, int> LogLevels { get; } = new Dictionary<SemanticLogLevel, int>();

            public void Log(SemanticLogLevel logLevel, Action<IObjectWriter> action)
            {
                LogCount++;
                LogLevels[logLevel] = LogLevels.GetOrDefault(logLevel) + 1;
            }

            public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
            {
                throw new NotSupportedException();
            }
        }

        public LogExceptionHandlerTests()
        {
            sut = new LogCommandHandler(log);
        }

        [Fact]
        public async Task Should_log_before_and_after_request()
        {
            var context = new CommandContext(command);

            await sut.HandleAsync(context, () =>
            {
                context.Complete(true);

                return TaskHelper.Done;
            });

            Assert.Equal(3, log.LogCount);
            Assert.Equal(3, log.LogLevels[SemanticLogLevel.Information]);
        }

        [Fact]
        public async Task Should_log_error_if_command_failed()
        {
            var context = new CommandContext(command);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await sut.HandleAsync(context, () => throw new InvalidOperationException());
            });

            Assert.Equal(3, log.LogCount);
            Assert.Equal(2, log.LogLevels[SemanticLogLevel.Information]);
            Assert.Equal(1, log.LogLevels[SemanticLogLevel.Error]);
        }

        [Fact]
        public async Task Should_log_if_command_is_not_handled()
        {
            var context = new CommandContext(command);

            await sut.HandleAsync(context, () => TaskHelper.Done);

            Assert.Equal(4, log.LogCount);
            Assert.Equal(3, log.LogLevels[SemanticLogLevel.Information]);
            Assert.Equal(1, log.LogLevels[SemanticLogLevel.Fatal]);
        }
    }
}