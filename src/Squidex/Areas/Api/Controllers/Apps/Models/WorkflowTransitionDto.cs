// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class WorkflowTransitionDto
    {
        /// <summary>
        /// The optional expression.
        /// </summary>
        public string? Expression { get; set; }

        /// <summary>
        /// The optional restricted role.
        /// </summary>
        public string? Role { get; set; }

        public static WorkflowTransitionDto? FromWorkflowTransition(WorkflowTransition transition)
        {
            if (transition == null)
            {
                return null;
            }

            return new WorkflowTransitionDto { Expression = transition.Expression, Role = transition.Role };
        }

        public WorkflowTransition ToTransition()
        {
            return new WorkflowTransition(Expression, Role);
        }
    }
}
