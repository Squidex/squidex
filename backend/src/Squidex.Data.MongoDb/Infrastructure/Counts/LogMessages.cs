// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Counts;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to update count for collection {collection}.")]
    public static partial void LogFailedToUpdateCount(ILogger logger, string collection, Exception exception);
}
