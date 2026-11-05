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
    [LoggerMessage(Level = LogLevel.Error, Message = "Insufficient privileges to apply the migrations of {context}. Ask an administrator to execute the following script manually and restart the application. Execute it as the application user with temporarily elevated privileges, so that the application remains the owner of the created objects:\n{sql}")]
    public static partial void LogMigrationPrivilegesMissing(ILogger logger, Exception exception, string context, string sql);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to apply the migrations of {context}. If the problem cannot be solved, ask an administrator to execute the following script manually and restart the application:\n{sql}")]
    public static partial void LogMigrationFailed(ILogger logger, Exception exception, string context, string sql);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Insufficient privileges to create index {name}. Ask an administrator to execute the following statements manually:\n{sql}")]
    public static partial void LogIndexPrivilegesMissing(ILogger logger, Exception exception, string name, string sql);
}
