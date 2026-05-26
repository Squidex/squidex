// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Web;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "An unexpected exception has occurred.")]
    public static partial void LogUnexpectedException(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Failed to send result.")]
    public static partial void LogFailedToSendResult(ILogger logger, Exception exception);
}
