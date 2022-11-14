// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public sealed class InMemoryCommandBus : ICommandBus
{
    private readonly NextDelegate pipeline;

    public InMemoryCommandBus(IEnumerable<ICommandMiddleware> middlewares)
    {
        var reverseMiddlewares = middlewares.Reverse().ToList();

        NextDelegate next = (c, ct) => Task.CompletedTask;

        foreach (var middleware in reverseMiddlewares)
        {
            next = Create(next, middleware);
        }

        pipeline = next;
    }

    private static NextDelegate Create(NextDelegate next, ICommandMiddleware middleware)
    {
        return (c, ct) => middleware.HandleAsync(c, next, ct);
    }

    public async Task<CommandContext> PublishAsync(ICommand command,
        CancellationToken ct)
    {
        Guard.NotNull(command);

        var context = new CommandContext(command, this);

        await pipeline(context, ct);

        return context;
    }
}
