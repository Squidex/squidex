// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
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
        public ReadOnlyCollection<string>? Roles { get; set; }

        public static WorkflowTransitionDto? FromWorkflowTransition(WorkflowTransition transition)
        {
            if (transition == null)
            {
                return null;
            }

            return new WorkflowTransitionDto { Expression = transition.Expression, Roles = transition.Roles };
        }

        public WorkflowTransition ToTransition()
        {
            return new WorkflowTransition(Expression, Roles);
        }
    }
}