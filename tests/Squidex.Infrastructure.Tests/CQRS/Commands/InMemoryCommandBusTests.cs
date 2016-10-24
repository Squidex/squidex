// ==========================================================================
//  InMemoryCommandBusTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public class InMemoryCommandBusTests
    {
        private readonly TestCommand command = new TestCommand();

        private sealed class TestCommand : ICommand
        {    
        }

        private sealed class HandledHandler : ICommandHandler
        {
            public ICommand LastCommand;

            public Task<bool> HandleAsync(CommandContext context)
            {
                LastCommand = context.Command;

                return Task.FromResult(true);
            }
        }

        private sealed class NonHandledHandler : ICommandHandler
        {
            public ICommand LastCommand;

            public Task<bool> HandleAsync(CommandContext context)
            {
                LastCommand = context.Command;

                return Task.FromResult(false);
            }
        }

        private sealed class ThrowHandledHandler : ICommandHandler
        {
            public ICommand LastCommand;

            public Task<bool> HandleAsync(CommandContext context)
            {
                LastCommand = context.Command;

                throw new InvalidOperationException();
            }
        }

        private sealed class AfterThrowHandler : ICommandHandler
        {
            public Exception LastException;

            public Task<bool> HandleAsync(CommandContext context)
            {
                LastException = context.Exception;

                return Task.FromResult(false);
            }
        }

        [Fact]
        public async Task Should_not_set_handled_if_no_handler_registered()
        {
            var sut = new InMemoryCommandBus(new ICommandHandler[0]);
            var ctx = await sut.PublishAsync(command);

            Assert.False(ctx.IsHandled);
        }

        [Fact]
        public async Task Should_not_set_succeeded_if_handler_returns_false()
        {
            var handler = new NonHandledHandler();

            var sut = new InMemoryCommandBus(new ICommandHandler[] { handler });
            var ctx = await sut.PublishAsync(command);

            Assert.Equal(command, handler.LastCommand);
            Assert.False(ctx.IsSucceeded);
            Assert.False(ctx.IsHandled);
            Assert.Null(ctx.Exception);
        }

        [Fact]
        public async Task Should_set_succeeded_if_handler_returns_true()
        {
            var handler = new HandledHandler();

            var sut = new InMemoryCommandBus(new ICommandHandler[] { handler });
            var ctx = await sut.PublishAsync(command);

            Assert.Equal(command, handler.LastCommand);
            Assert.True(ctx.IsSucceeded);
            Assert.True(ctx.IsHandled);
            Assert.Null(ctx.Exception);
        }

        [Fact]
        public async Task Should_throw_and_call_all_handlers_if_first_handler_fails()
        {
            var handler1 = new ThrowHandledHandler();
            var handler2 = new AfterThrowHandler();

            var sut = new InMemoryCommandBus(new ICommandHandler[] { handler1, handler2 });

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.PublishAsync(command));

            Assert.Equal(command, handler1.LastCommand);
            Assert.IsType<InvalidOperationException>(handler2.LastException);
        }
    }
}
