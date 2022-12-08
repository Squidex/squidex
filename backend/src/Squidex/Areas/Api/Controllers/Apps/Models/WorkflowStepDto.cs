// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using NoUpdateType = Squidex.Domain.Apps.Core.Contents.NoUpdate;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class WorkflowStepDto
{
    /// <summary>
    /// The transitions.
    /// </summary>
    [LocalizedRequired]
    public Dictionary<Status, WorkflowTransitionDto> Transitions { get; set; }

    /// <summary>
    /// The optional color.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// True if the content should be validated when moving to this step.
    /// </summary>
    public bool Validate { get; set; }

    /// <summary>
    /// Indicates if updates should not be allowed.
    /// </summary>
    public bool NoUpdate { get; set; }

    /// <summary>
    /// Optional expression that must evaluate to true when you want to prevent updates.
    /// </summary>
    public string? NoUpdateExpression { get; set; }

    /// <summary>
    /// Optional list of roles to restrict the updates for users with these roles.
    /// </summary>
    public string[]? NoUpdateRoles { get; set; }

    public static WorkflowStepDto FromDomain(WorkflowStep step)
    {
        var result = SimpleMapper.Map(step, new WorkflowStepDto
        {
            Transitions = step.Transitions.ToDictionary(
                y => y.Key,
                y => WorkflowTransitionDto.FromDomain(y.Value))
        });

        if (step.NoUpdate != null)
        {
            result.NoUpdate = true;
            result.NoUpdateExpression = step.NoUpdate.Expression;
            result.NoUpdateRoles = step.NoUpdate.Roles?.ToArray();
        }

        return result;
    }

    public WorkflowStep ToWorkflowStep()
    {
        return new WorkflowStep(
            Transitions?.ToReadonlyDictionary(
                y => y.Key,
                y => y.Value?.ToWorkflowTransition()!),
            Color,
            NoUpdate ?
                NoUpdateType.When(NoUpdateExpression, NoUpdateRoles) :
                null,
            Validate);
    }
}
