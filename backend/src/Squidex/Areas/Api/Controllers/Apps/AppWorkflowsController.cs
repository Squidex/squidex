// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppWorkflowsController : ApiController
    {
        private readonly IWorkflowsValidator workflowsValidator;

        public AppWorkflowsController(ICommandBus commandBus, IWorkflowsValidator workflowsValidator)
            : base(commandBus)
        {
            this.workflowsValidator = workflowsValidator;
        }

        /// <summary>
        /// Get app workflow.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Workflows returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/workflows/")]
        [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppWorkflowsRead)]
        [ApiCosts(0)]
        public IActionResult GetWorkflows(string app)
        {
            var response = Deferred.AsyncResponse(() =>
            {
                return GetResponse(App);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Create a workflow.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The new workflow.</param>
        /// <returns>
        /// 200 => Workflow created.
        /// 400 => Workflow request not valid.
        /// 404 => Workflow or app not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/workflows/")]
        [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppWorkflowsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostWorkflow(string app, [FromBody] AddWorkflowDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Update a workflow.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the workflow to update.</param>
        /// <param name="request">The new workflow.</param>
        /// <returns>
        /// 200 => Workflow updated.
        /// 400 => Workflow request not valid.
        /// 404 => Workflow or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/workflows/{id}")]
        [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppWorkflowsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutWorkflow(string app, DomainId id, [FromBody] UpdateWorkflowDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Delete a workflow.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the workflow to update.</param>
        /// <returns>
        /// 200 => Workflow deleted.
        /// 404 => Workflow or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/workflows/{id}")]
        [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppWorkflowsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteWorkflow(string app, DomainId id)
        {
            var command = new DeleteWorkflow { WorkflowId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        private async Task<WorkflowsDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = await GetResponse(result);

            return response;
        }

        private async Task<WorkflowsDto> GetResponse(IAppEntity result)
        {
            return await WorkflowsDto.FromAppAsync(workflowsValidator, result, Resources);
        }
    }
}
