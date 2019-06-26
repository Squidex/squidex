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
    public sealed class UpsertWorkflowDto
    {
        /// <summary>
        /// The workflow steps.
        /// </summary>
        [Required]
        public Dictionary<Status, WorkflowStepDto> Steps { get; set; }

        /// <summary>
        /// The initial step.
        /// </summary>
        public Status Initial { get; set; }

        public ConfigureWorkflow ToCommand()
        {
            var workflow = new Workflow(
                Steps?.ToDictionary(
                    x => x.Key,
                    x => new WorkflowStep(
                        x.Value?.Transitions.ToDictionary(
                            y => x.Key,
                            y => new WorkflowTransition(y.Value.Expression, y.Value.Role)) ?? WorkflowStep.EmptyTransitions,
                        x.Value.Color,
                        x.Value.NoUpdate)) ?? Workflow.EmptySteps,
                Initial);

            return new ConfigureWorkflow { Workflow = workflow };
        }
    }
}
