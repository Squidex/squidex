// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure;

namespace Squidex.Web;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "An unexpected exception has occurred.")]
    public static partial void LogUnexpectedException(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Failed to send result.")]
    public static partial void LogFailedToSendResult(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot find app with the given name {name}.")]
    public static partial void LogCannotFindAppByName(ILogger logger, string name);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Authenticated user has no permission to access the app {name} with ID {id}.")]
    public static partial void LogNoPermissionToAccessApp(ILogger logger, DomainId id, string name);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot find team with the given id {id}.")]
    public static partial void LogCannotFindTeamById(ILogger logger, string id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Authenticated user has no permission to access the team with ID {id}.")]
    public static partial void LogNoPermissionToAccessTeam(ILogger logger, DomainId id);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error while handling api key.")]
    public static partial void LogErrorHandlingApiKey(ILogger logger, Exception exception);
}
