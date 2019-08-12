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
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class WorkflowStepDto
    {
        /// <summary>
        /// The transitions.
        /// </summary>
        [Required]
        public Dictionary<Status, WorkflowTransitionDto> Transitions { get; set; }

        /// <summary>
        /// The optional color.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Indicates if updates should not be allowed.
        /// </summary>
        public bool NoUpdate { get; set; }

        public static WorkflowStepDto FromWorkflowStep(WorkflowStep step)
        {
            if (step == null)
            {
                return null;
            }

            return SimpleMapper.Map(step, new WorkflowStepDto
            {
                Transitions = step.Transitions.ToDictionary(
                    y => y.Key,
                    y => WorkflowTransitionDto.FromWorkflowTransition(y.Value))
            });
        }

        public WorkflowStep ToStep()
        {
            return new WorkflowStep(
                Transitions?.ToDictionary(
                    y => y.Key,
                    y => y.Value?.ToTransition()),
                Color, NoUpdate);
        }
    }
}
