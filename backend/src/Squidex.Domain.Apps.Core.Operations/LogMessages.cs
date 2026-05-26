// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Domain.Apps.Core;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create rule job.")]
    public static partial void LogFailedToCreateRuleJob(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create rule jobs from trigger.")]
    public static partial void LogFailedToCreateRuleJobsFromTrigger(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create rule jobs from event.")]
    public static partial void LogFailedToCreateRuleJobsFromEvent(ILogger logger, Exception exception);
}
