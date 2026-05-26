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
    public static partial void LogKafkaError(ILogger logger, object code, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to enrich asset.")]
    public static partial void LogFailedToEnrichAsset(ILogger logger, Exception exception);
}
