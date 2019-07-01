// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class UpdateWorkflowDto
    {
        /// <summary>
        /// The name of the workflow.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The workflow steps.
        /// </summary>
        [Required]
        public Dictionary<Status, WorkflowStepDto> Steps { get; set; }

        /// <summary>
        /// The initial step.
        /// </summary>
        public Status Initial { get; set; }

        public UpdateWorkflow ToCommand()
        {
            var workflow = new Workflow(
                Steps?.ToDictionary(
                    x => x.Key,
                    x => new WorkflowStep(
                        x.Value?.Transitions.ToDictionary(
                            y => y.Key,
                            y => new WorkflowTransition(y.Value.Expression, y.Value.Role)),
                        x.Value.Color,
                        x.Value.NoUpdate)),
                Initial, Name);

            return new UpdateWorkflow { Workflow = workflow };
        }
    }
}
