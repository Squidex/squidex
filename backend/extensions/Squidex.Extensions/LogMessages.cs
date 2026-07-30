// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Extensions;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Kafka error with {code} and {reason}.")]
#pragma warning disable LOGGEN036 // A value being logged doesn't have an effective way to be converted into a string
    public static partial void LogKafkaError(ILogger logger, object code, string reason);
#pragma warning restore LOGGEN036 // A value being logged doesn't have an effective way to be converted into a string

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to enrich asset.")]
    public static partial void LogFailedToEnrichAsset(ILogger logger, Exception exception);
}
