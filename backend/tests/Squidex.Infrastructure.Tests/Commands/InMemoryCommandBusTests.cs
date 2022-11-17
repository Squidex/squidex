// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public class InMemoryCommandBusTests
{
    private readonly ICommand command = A.Fake<ICommand>();

    private sealed class HandledHandler : ICommandMiddleware
    {
        public ICommand LastCommand { get; private set; }

        public Task HandleAsync(CommandContext context, NextDelegate next,
            CancellationToken ct)
        {
            LastCommand = context.Command;

            context.Complete(true);

            return Task.FromResult(true);
        }
    }

    private sealed class NonHandledHandler : ICommandMiddleware
    {
        public ICommand LastCommand { get; private set; }

        public Task HandleAsync(CommandContext context, NextDelegate next,
            CancellationToken ct)
        {
            LastCommand = context.Command;

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowHandledHandler : ICommandMiddleware
    {
        public ICommand LastCommand { get; private set; }

        public Task HandleAsync(CommandContext context, NextDelegate next,
            CancellationToken ct)
        {
            LastCommand = context.Command;

            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task Should_not_set_handled_if_no_handler_registered()
    {
        var sut = new InMemoryCommandBus(Array.Empty<ICommandMiddleware>());

        var context = await sut.PublishAsync(command, default);

        Assert.False(context.IsCompleted);
    }

    [Fact]
    public async Task Should_not_set_succeeded_if_handler_returns_false()
    {
        var handler = new NonHandledHandler();

        var sut = new InMemoryCommandBus(new ICommandMiddleware[] { handler });

        var context = await sut.PublishAsync(command, default);

        Assert.Equal(command, handler.LastCommand);
        Assert.False(context.IsCompleted);
    }

    [Fact]
    public async Task Should_set_succeeded_if_handler_marks_completed()
    {
        var handler = new HandledHandler();

        var sut = new InMemoryCommandBus(new ICommandMiddleware[] { handler });

        var context = await sut.PublishAsync(command, default);

        Assert.Equal(command, handler.LastCommand);
        Assert.True(context.IsCompleted);
    }

    [Fact]
    public async Task Should_throw_and_call_all_handlers_if_first_handler_fails()
    {
        var handler = new ThrowHandledHandler();

        var sut = new InMemoryCommandBus(new ICommandMiddleware[] { handler });

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.PublishAsync(command, default));

        Assert.Equal(command, handler.LastCommand);
    }
}
