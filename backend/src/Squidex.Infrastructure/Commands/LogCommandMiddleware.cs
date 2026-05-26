// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Commands;

public sealed partial class LogCommandMiddleware(ILogger<LogCommandMiddleware> log) : ICommandMiddleware
{
    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        var type = context.Command.GetType();

        try
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                LogCommandStarted(log, type, context.ContextId);
            }

            var watch = ValueStopwatch.StartNew();
            try
            {
                await next(context, ct);

                LogCommandSucceeded(log, type, context.ContextId);
            }
            finally
            {
                LogCommandCompleted(log, type, context.ContextId, watch.Stop());
            }
        }
        catch (Exception ex)
        {
            LogCommandFailed(log, type, context.ContextId, ex);
            throw;
        }

        if (!context.IsCompleted)
        {
            LogCommandNotHandled(log, type, context.ContextId);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Command {command} with ID {id} started.")]
    private static partial void LogCommandStarted(ILogger logger, Type command, DomainId id);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Command {command} with ID {id} succeeded.")]
    private static partial void LogCommandSucceeded(ILogger logger, Type command, DomainId id);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Command {command} with ID {id} completed after {time}ms.")]
    private static partial void LogCommandCompleted(ILogger logger, Type command, DomainId id, long time);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Command {command} with ID {id} failed.")]
    private static partial void LogCommandFailed(ILogger logger, Type command, DomainId id, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "Command {command} with ID {id} not handled.")]
    private static partial void LogCommandNotHandled(ILogger logger, Type command, DomainId id);
}
