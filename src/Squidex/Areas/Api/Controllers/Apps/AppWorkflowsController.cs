// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
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
        public AppWorkflowsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get app workflow.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => App workflows returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/workflow/")]
        [ProducesResponseType(typeof(WorkflowResponseDto), 200)]
        [ApiPermission(Permissions.AppWorkflowsRead)]
        [ApiCosts(0)]
        public IActionResult GetWorkflow(string app)
        {
            var response = WorkflowResponseDto.FromApp(App, this);

            Response.Headers[HeaderNames.ETag] = App.Version.ToString();

            return Ok(response);
        }

        /// <summary>
        /// Configure workflow of the app.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The new workflow.</param>
        /// <returns>
        /// 200 => Workflow configured.
        /// 400 => Workflow is not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/workflow/")]
        [ProducesResponseType(typeof(WorkflowResponseDto), 200)]
        [ApiPermission(Permissions.AppWorkflowsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutWorkflow(string app, [FromBody] UpsertWorkflowDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        private async Task<WorkflowResponseDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = WorkflowResponseDto.FromApp(result, this);

            return response;
        }
    }
}
