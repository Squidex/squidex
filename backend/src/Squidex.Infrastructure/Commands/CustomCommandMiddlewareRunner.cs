// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands;

public sealed class CustomCommandMiddlewareRunner(IEnumerable<ICustomCommandMiddleware> extensions) : ICommandMiddleware
{
    private readonly IEnumerable<ICustomCommandMiddleware> extensions = extensions.Reverse().ToList();

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        foreach (var handler in extensions)
        {
            next = Join(handler, next);
        }

        await next(context, ct);
    }

    private static NextDelegate Join(ICommandMiddleware handler, NextDelegate next)
    {
        return (context, ct) => handler.HandleAsync(context, next, ct);
    }
}
