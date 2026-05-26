// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Commands;

public sealed class LogCommandMiddleware(ILogger<LogCommandMiddleware> log) : ICommandMiddleware
{
    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        var type = context.Command.GetType();

        try
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                LogMessages.LogCommandStarted(log, type, context.ContextId);
            }

            var watch = ValueStopwatch.StartNew();
            try
            {
                await next(context, ct);

                LogMessages.LogCommandSucceeded(log, type, context.ContextId);
            }
            finally
            {
                LogMessages.LogCommandCompleted(log, type, context.ContextId, watch.Stop());
            }
        }
        catch (Exception ex)
        {
            LogMessages.LogCommandFailed(log, type, context.ContextId, ex);
            throw;
        }

        if (!context.IsCompleted)
        {
            LogMessages.LogCommandNotHandled(log, type, context.ContextId);
        }
    }
}
