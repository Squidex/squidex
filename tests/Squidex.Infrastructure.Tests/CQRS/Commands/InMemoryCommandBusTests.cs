// ==========================================================================
//  InMemoryCommandBusTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class InMemoryCommandBusTests
    {
        private readonly ICommand command = A.Fake<ICommand>();

        private sealed class HandledHandler : ICommandMiddleware
        {
            public ICommand LastCommand;

            public Task HandleAsync(CommandContext context, Func<Task> next)
            {
                LastCommand = context.Command;

                context.Complete(true);

                return TaskHelper.True;
            }
        }

        private sealed class NonHandledHandler : ICommandMiddleware
        {
            public ICommand LastCommand;

            public Task HandleAsync(CommandContext context, Func<Task> next)
            {
                LastCommand = context.Command;

                return TaskHelper.Done;
            }
        }

        private sealed class ThrowHandledHandler : ICommandMiddleware
        {
            public ICommand LastCommand;

            public Task HandleAsync(CommandContext context, Func<Task> next)
            {
                LastCommand = context.Command;

                throw new InvalidOperationException();
            }
        }

        [Fact]
        public async Task Should_not_set_handled_if_no_handler_registered()
        {
            var sut = new InMemoryCommandBus(new ICommandMiddleware[0]);
            var ctx = await sut.PublishAsync(command);

            Assert.False(ctx.IsCompleted);
        }

        [Fact]
        public async Task Should_not_set_succeeded_if_handler_returns_false()
        {
            var handler = new NonHandledHandler();

            var sut = new InMemoryCommandBus(new ICommandMiddleware[] { handler });
            var ctx = await sut.PublishAsync(command);

            Assert.Equal(command, handler.LastCommand);
            Assert.False(ctx.IsCompleted);
        }

        [Fact]
        public async Task Should_set_succeeded_if_handler_marks_completed()
        {
            var handler = new HandledHandler();

            var sut = new InMemoryCommandBus(new ICommandMiddleware[] { handler });
            var ctx = await sut.PublishAsync(command);

            Assert.Equal(command, handler.LastCommand);
            Assert.True(ctx.IsCompleted);
        }

        [Fact]
        public async Task Should_throw_and_call_all_handlers_if_first_handler_fails()
        {
            var handler = new ThrowHandledHandler();

            var sut = new InMemoryCommandBus(new ICommandMiddleware[] { handler });

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.PublishAsync(command));

            Assert.Equal(command, handler.LastCommand);
        }
    }
}
