// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.Commands;

public delegate Task NextDelegate(CommandContext context, CancellationToken ct);

public interface ICommandMiddleware
{
    Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct);
}
