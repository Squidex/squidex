// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class FlowExecutionStateDto
{
    /// <summary>
    /// The actual definition of the the steps to be executed.
    /// </summary>
    public FlowDefinitionDto Definition { get; set; }

    /// <summary>
    /// The context.
    /// </summary>
    public object Context { get; set; }

    /// <summary>
    /// The state of each step.
    /// </summary>
    public Dictionary<Guid, FlowExecutionStepStateDto> Steps { get; set; } = [];

    /// <summary>
    /// The next step to be executed.
    /// </summary>
    public Guid NextStepId { get; set; }

    /// <summary>
    /// THe time when the next step will be executed.
    /// </summary>
    public Instant? NextRun { get; set; }

    /// <summary>
    /// The creation time.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The completion time.
    /// </summary>
    public Instant Completed { get; set; }

    /// <summary>
    /// The overall status.
    /// </summary>
    public FlowExecutionStatus Status { get; set; }

    public static FlowExecutionStateDto FromDomain(FlowExecutionState<FlowEventContext> state)
    {
        return SimpleMapper.Map(state, new FlowExecutionStateDto
        {
            Steps = state.Steps.ToDictionary(
                x => x.Key,
                x => FlowExecutionStepStateDto.FromDomain(x.Value)),
            Definition = FlowDefinitionDto.FromDomain(state.Definition),
        });
    }
}
