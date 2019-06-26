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
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class WorkflowDto : Resource
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

        public static WorkflowDto FromWorkflow(Workflow workflow, ApiController controller, string app)
        {
            var result = new WorkflowDto
            {
                Steps = workflow.Steps.ToDictionary(
                    x => x.Key,
                    x => SimpleMapper.Map(x.Value, new WorkflowStepDto
                    {
                        Transitions = x.Value.Transitions.ToDictionary(
                            y => y.Key,
                            y => new WorkflowTransitionDto { Expression = y.Value.Expression, Role = y.Value.Role })
                    })),
                Initial = workflow.Initial
            };

            return result.CreateLinks(controller, app);
        }

        private WorkflowDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            if (controller.HasPermission(Permissions.AppWorkflowsUpdate, app))
            {
                AddPutLink("update", controller.Url<AppWorkflowsController>(x => nameof(x.PutWorkflow), values));
            }

            return this;
        }
    }
}
