// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
        /// The workflow id.
        /// </summary>
        public Guid Id { get; set; }

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
        /// The schema ids.
        /// </summary>
        public IReadOnlyList<Guid> SchemaIds { get; set; }

        /// <summary>
        /// The initial step.
        /// </summary>
        public Status Initial { get; set; }

        public static WorkflowDto FromWorkflow(Guid id, Workflow workflow, ApiController controller, string app)
        {
            var result = SimpleMapper.Map(workflow, new WorkflowDto { Id = id });

            result.Steps = workflow.Steps.ToDictionary(
                x => x.Key,
                x => SimpleMapper.Map(x.Value, new WorkflowStepDto
                {
                    Transitions = x.Value.Transitions.ToDictionary(
                        y => y.Key,
                        y => new WorkflowTransitionDto { Expression = y.Value.Expression, Role = y.Value.Role })
                }));

            return result.CreateLinks(controller, app, id);
        }

        private WorkflowDto CreateLinks(ApiController controller, string app, Guid id)
        {
            var values = new { app, id };

            if (controller.HasPermission(Permissions.AppWorkflowsUpdate, app))
            {
                AddPutLink("update", controller.Url<AppWorkflowsController>(x => nameof(x.PutWorkflow), values));
            }

            if (controller.HasPermission(Permissions.AppWorkflowsDelete, app))
            {
                AddDeleteLink("delete", controller.Url<AppWorkflowsController>(x => nameof(x.DeleteWorkflow), values));
            }

            return this;
        }
    }
}
