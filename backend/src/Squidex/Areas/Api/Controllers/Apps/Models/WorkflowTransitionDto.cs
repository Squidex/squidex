// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class WorkflowTransitionDto
{
    /// <summary>
    /// The optional expression.
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// The optional restricted role.
    /// </summary>
    public string[]? Roles { get; set; }

    public static WorkflowTransitionDto FromDomain(WorkflowTransition transition)
    {
        var result = new WorkflowTransitionDto { Expression = transition.Expression, Roles = transition.Roles?.ToArray() };

        return result;
    }

    public WorkflowTransition ToWorkflowTransition()
    {
        return WorkflowTransition.When(Expression, Roles);
    }
}
