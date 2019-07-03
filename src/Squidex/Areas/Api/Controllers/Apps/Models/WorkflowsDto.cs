// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class WorkflowsDto : Resource
    {
        /// <summary>
        /// The workflow.
        /// </summary>
        [Required]
        public WorkflowDto[] Items { get; set; }

        /// <summary>
        /// The errros that should be fixed.
        /// </summary>
        [Required]
        public string[] Errors { get; set; }

        public static async Task<WorkflowsDto> FromAppAsync(IWorkflowsValidator workflowsValidator, IAppEntity app, ApiController controller)
        {
            var result = new WorkflowsDto
            {
                Items = app.Workflows.Select(x => WorkflowDto.FromWorkflow(x.Key, x.Value, controller, app.Name)).ToArray(),
            };

            var errors = await workflowsValidator.ValidateAsync(app.Id, app.Workflows);

            result.Errors = errors.ToArray();

            return result.CreateLinks(controller, app.Name);
        }

        private WorkflowsDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<AppWorkflowsController>(x => nameof(x.GetWorkflows), values));

            if (controller.HasPermission(Permissions.AppWorkflowsCreate, app))
            {
                AddPostLink("create", controller.Url<AppWorkflowsController>(x => nameof(x.PostWorkflow), values));
            }

            return this;
        }
    }
}
