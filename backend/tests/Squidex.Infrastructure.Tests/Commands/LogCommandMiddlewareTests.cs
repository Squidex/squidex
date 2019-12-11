﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class LogCommandMiddlewareTests
    {
        private readonly MyLog log = new MyLog();
        private readonly LogCommandMiddleware sut;
        private readonly ICommand command = A.Dummy<ICommand>();
        private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();

        private sealed class MyLog : ISemanticLog
        {
            public Dictionary<SemanticLogLevel, int> LogLevels { get; } = new Dictionary<SemanticLogLevel, int>();

            public void Log<T>(SemanticLogLevel logLevel, T context, Action<T, IObjectWriter> action)
            {
                LogLevels[logLevel] = LogLevels.GetOrDefault(logLevel) + 1;
            }

            public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
            {
                throw new NotSupportedException();
            }
        }

        public LogCommandMiddlewareTests()
        {
            sut = new LogCommandMiddleware(log);
        }

        [Fact]
        public async Task Should_log_before_and_after_request()
        {
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context, () =>
            {
                context.Complete(true);

                return TaskHelper.Done;
            });

            Assert.Equal(log.LogLevels, new Dictionary<SemanticLogLevel, int>
            {
                [SemanticLogLevel.Debug] = 1,
                [SemanticLogLevel.Information] = 2
            });
        }

        [Fact]
        public async Task Should_log_error_if_command_failed()
        {
            var context = new CommandContext(command, commandBus);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await sut.HandleAsync(context, () => throw new InvalidOperationException());
            });

            Assert.Equal(log.LogLevels, new Dictionary<SemanticLogLevel, int>
            {
                [SemanticLogLevel.Debug] = 1,
                [SemanticLogLevel.Information] = 1,
                [SemanticLogLevel.Error] = 1,
            });
        }

        [Fact]
        public async Task Should_log_if_command_is_not_handled()
        {
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context, () => TaskHelper.Done);

            Assert.Equal(log.LogLevels, new Dictionary<SemanticLogLevel, int>
            {
                [SemanticLogLevel.Debug] = 1,
                [SemanticLogLevel.Information] = 2,
                [SemanticLogLevel.Fatal] = 1,
            });
        }
    }
}