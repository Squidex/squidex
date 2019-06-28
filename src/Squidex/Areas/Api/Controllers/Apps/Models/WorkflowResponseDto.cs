// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class WorkflowResponseDto : Resource
    {
        /// <summary>
        /// The workflow.
        /// </summary>
        [Required]
        public WorkflowDto Workflow { get; set; }

        public static WorkflowResponseDto FromApp(IAppEntity app, ApiController controller)
        {
            var result = new WorkflowResponseDto
            {
                Workflow = WorkflowDto.FromWorkflow(app.Workflows.GetFirst(), controller, app.Name)
            };

            return result;
        }
    }
}
