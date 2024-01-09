// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Jobs;

namespace Squidex.Areas.Api.Controllers.Jobs.Models;

public class JobLogMessageDto
{
    /// <summary>
    /// The timestamp.
    /// </summary>
    public Instant Timestamp { get; set; }

    /// <summary>
    /// The log message.
    /// </summary>
    public string Message { get; set; }

    public static JobLogMessageDto FromDomain(JobLogMessage source)
    {
        return new JobLogMessageDto { Timestamp = source.Timestamp, Message = source.Message };
    }
}
