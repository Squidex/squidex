// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class CustomCommandMiddlewareRunnerTests
    {
        public sealed class Command : ICommand
        {
            public List<int> Values { get; set; } = new List<int>();

            public long ExpectedVersion { get; set; }
        }

        public sealed class CustomMiddleware : ICustomCommandMiddleware
        {
            private readonly int value;

            public CustomMiddleware(int value)
            {
                this.value = value;
            }

            public Task HandleAsync(CommandContext context, NextDelegate next)
            {
                if (context.Command is Command command)
                {
                    command.Values.Add(value);
                }

                return next(context);
            }
        }

        [Fact]
        public async Task Should_run_extensions_in_right_order()
        {
            var command = new Command();
            var context = new CommandContext(command, A.Fake<ICommandBus>());

            var sut = new CustomCommandMiddlewareRunner(new[]
            {
                new CustomMiddleware(10),
                new CustomMiddleware(12),
                new CustomMiddleware(14)
            });

            var isNextCalled = false;

            await sut.HandleAsync(context, c =>
            {
                isNextCalled = true;

                Assert.Equal(new[] { 10, 12, 14 }, command.Values);

                return Task.CompletedTask;
            });

            Assert.True(isNextCalled);
        }
    }
}
