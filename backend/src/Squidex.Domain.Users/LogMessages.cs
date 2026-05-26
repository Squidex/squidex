// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Domain.Users;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to cleanup user after creation failed.")]
    public static partial void LogFailedToCleanupUser(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Identity operation failed: {errorMessage}.")]
    public static partial void LogIdentityOperationFailed(ILogger logger, string errorMessage);
}
