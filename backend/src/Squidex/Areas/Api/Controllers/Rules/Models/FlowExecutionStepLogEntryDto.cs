// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class FlowExecutionStepLogEntryDto
{
    /// <summary>
    /// The timestamp.
    /// </summary>
    public Instant Timestamp { get; set; }

    /// <summary>
    /// The log message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// A detailed dump.
    /// </summary>
    public string? Dump { get; set; }

    public static FlowExecutionStepLogEntryDto FromDomain(FlowExecutionStepLogEntry log)
    {
        return SimpleMapper.Map(log, new FlowExecutionStepLogEntryDto());
    }
}
