// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Command {command} with ID {id} started.")]
    public static partial void LogCommandStarted(ILogger logger, Type command, DomainId id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Command {command} with ID {id} succeeded.")]
    public static partial void LogCommandSucceeded(ILogger logger, Type command, DomainId id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Command {command} with ID {id} completed after {time}ms.")]
    public static partial void LogCommandCompleted(ILogger logger, Type command, DomainId id, long time);

    [LoggerMessage(Level = LogLevel.Error, Message = "Command {command} with ID {id} failed.")]
    public static partial void LogCommandFailed(ILogger logger, Type command, DomainId id, Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Command {command} with ID {id} not handled.")]
    public static partial void LogCommandNotHandled(ILogger logger, Type command, DomainId id);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Failed to complete consumer.")]
    public static partial void LogFailedToCompleteConsumer(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to handle event.")]
    public static partial void LogFailedToHandleEvent(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Failed to update consumer {consumer} at position {position} from {caller}.")]
    public static partial void LogFailedToUpdateConsumer(ILogger logger, string consumer, string? position, string? caller, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Event consumer {consumer} reset started")]
    public static partial void LogEventConsumerResetStarted(ILogger logger, string consumer);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Event consumer {consumer} reset completed after {time}ms.")]
    public static partial void LogEventConsumerResetCompleted(ILogger logger, string consumer, long time);

    [LoggerMessage(Level = LogLevel.Information, Message = "Migration {migration} started.")]
    public static partial void LogMigrationStarted(ILogger logger, string migration);

    [LoggerMessage(Level = LogLevel.Information, Message = "Migration {migration} completed after {time}ms.")]
    public static partial void LogMigrationCompleted(ILogger logger, string migration, long time);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Migration {migration} failed.")]
    public static partial void LogMigrationFailed(ILogger logger, string migration, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Could not acquire lock to start migrating. Trying again in {time}ms.")]
    public static partial void LogMigrationLockRetry(ILogger logger, int time);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to track usage in background.")]
    public static partial void LogTrackUsageFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to repair snapshot for domain object of type {type} with ID {id}.")]
    public static partial void LogFailedToRepairDomainObjectSnapshot(ILogger logger, Type type, DomainId id, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Found corrupt domain object of type {type} with ID {id}.")]
    public static partial void LogFoundCorruptDomainObject(ILogger logger, Type type, DomainId id, Exception exception);
}
