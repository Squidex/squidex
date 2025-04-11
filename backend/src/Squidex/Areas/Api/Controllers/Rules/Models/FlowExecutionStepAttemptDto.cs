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

public sealed class FlowExecutionStepAttemptDto
{
    /// <summary>
    /// The log messages.
    /// </summary>
    public List<FlowExecutionStepLogEntryDto> Log { get; set; } = [];

    /// <summary>
    /// The time when the attempt has been started.
    /// </summary>
    public Instant Started { get; set; }

    /// <summary>
    /// The time when the attempt has been completed.
    /// </summary>
    public Instant Completed { get; set; }

    /// <summary>
    /// The error, if there is any.
    /// </summary>
    public string? Error { get; set; }

    public static FlowExecutionStepAttemptDto FromDomain(FlowExecutionStepAttempt attempt)
    {
        return SimpleMapper.Map(attempt, new FlowExecutionStepAttemptDto
        {
            Log = attempt.Log.Select(FlowExecutionStepLogEntryDto.FromDomain).ToList(),
        });
    }
}
