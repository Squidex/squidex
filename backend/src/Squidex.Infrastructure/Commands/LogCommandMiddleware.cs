// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Commands;

public sealed class LogCommandMiddleware : ICommandMiddleware
{
    private readonly ILogger<LogCommandMiddleware> log;

    public LogCommandMiddleware(ILogger<LogCommandMiddleware> log)
    {
        this.log = log;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        var type = context.Command.GetType();

        try
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug("Command {command} with ID {id} started.", type, context.ContextId);
            }

            var watch = ValueStopwatch.StartNew();
            try
            {
                await next(context, ct);

                log.LogInformation("Command {command} with ID {id} succeeded.", type, context.ContextId);
            }
            finally
            {
                log.LogInformation("Command {command} with ID {id} completed after {time}ms.", type, context.ContextId, watch.Stop());
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Command {command} with ID {id} failed.", type, context.ContextId);
            throw;
        }

        if (!context.IsCompleted)
        {
            log.LogCritical("Command {command} with ID {id} not handled.", type, context.ContextId);
        }
    }
}
