// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create administrator.")]
    public static partial void LogFailedToCreateAdministrator(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to return users, returning empty results.")]
    public static partial void LogFailedToReturnUsers(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to return user, returning empty results.")]
    public static partial void LogFailedToReturnUser(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to return user picture, returning fallback image.")]
    public static partial void LogFailedToReturnUserPicture(ILogger logger, Exception exception);
}
