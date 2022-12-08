// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Infrastructure.Commands;

public sealed class EnrichWithTimestampCommandMiddleware : ICommandMiddleware
{
    public IClock Clock { get; set; } = SystemClock.Instance;

    public Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is ITimestampCommand timestampCommand)
        {
            timestampCommand.Timestamp = Clock.GetCurrentInstant();
        }

        return next(context, ct);
    }
}
