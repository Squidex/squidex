// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure.Commands;

public sealed class ReadonlyCommandMiddleware : ICommandMiddleware
{
    private readonly ReadonlyOptions options;

    public ReadonlyCommandMiddleware(IOptions<ReadonlyOptions> options)
    {
        this.options = options.Value;
    }

    public Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (options.IsReadonly)
        {
            throw new DomainException(T.Get("common.readonlyMode"));
        }

        return next(context, ct);
    }
}
