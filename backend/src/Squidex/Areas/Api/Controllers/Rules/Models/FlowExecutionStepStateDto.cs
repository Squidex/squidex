// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class FlowExecutionStepStateDto
{
    /// <summary>
    /// The status of this step.
    /// </summary>
    public FlowExecutionStatus Status { get; set; }

    /// <summary>
    /// Indicates if the step has already been prepared (happens once for all attempts).
    /// </summary>
    public bool IsPrepared { get; set; }

    /// <summary>
    /// The different attempts.
    /// </summary>
    public List<FlowExecutionStepAttemptDto> Attempts { get; set; } = [];

    public static FlowExecutionStepStateDto FromDomain(FlowExecutionStepState source)
    {
        return SimpleMapper.Map(source, new FlowExecutionStepStateDto
        {
            Attempts = source.Attempts.Select(FlowExecutionStepAttemptDto.FromDomain).ToList(),
        });
    }
}
