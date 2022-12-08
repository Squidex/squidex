// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
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
    /// <response code="200">Workflows returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/workflows/")]
    [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppWorkflowsRead)]
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
    /// <response code="200">Workflow created.</response>.
    /// <response code="400">Workflow request not valid.</response>.
    /// <response code="404">Workflow or app not found.</response>.
    [HttpPost]
    [Route("apps/{app}/workflows/")]
    [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppWorkflowsUpdate)]
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
    /// <param name="id">The ID of the workflow to update.</param>
    /// <param name="request">The new workflow.</param>
    /// <response code="200">Workflow updated.</response>.
    /// <response code="400">Workflow request not valid.</response>.
    /// <response code="404">Workflow or app not found.</response>.
    [HttpPut]
    [Route("apps/{app}/workflows/{id}")]
    [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppWorkflowsUpdate)]
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
    /// <param name="id">The ID of the workflow to update.</param>
    /// <response code="200">Workflow deleted.</response>.
    /// <response code="404">Workflow or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/workflows/{id}")]
    [ProducesResponseType(typeof(WorkflowsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppWorkflowsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteWorkflow(string app, DomainId id)
    {
        var command = new DeleteWorkflow { WorkflowId = id };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    private async Task<WorkflowsDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<IAppEntity>();
        var response = await GetResponse(result);

        return response;
    }

    private async Task<WorkflowsDto> GetResponse(IAppEntity result)
    {
        return await WorkflowsDto.FromAppAsync(workflowsValidator, result, Resources);
    }
}
