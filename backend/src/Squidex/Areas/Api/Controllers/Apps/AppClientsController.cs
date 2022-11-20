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
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Apps))]
public sealed class AppClientsController : ApiController
{
    public AppClientsController(ICommandBus commandBus)
        : base(commandBus)
    {
    }

    /// <summary>
    /// Get app clients.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Clients returned.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// Gets all configured clients for the app with the specified name.
    /// </remarks>
    [HttpGet]
    [Route("apps/{app}/clients/")]
    [ProducesResponseType(typeof(ClientsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppClientsRead)]
    [ApiCosts(0)]
    public IActionResult GetClients(string app)
    {
        var response = Deferred.Response(() =>
        {
            return GetResponse(App);
        });

        Response.Headers[HeaderNames.ETag] = App.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Create a new app client.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">Client object that needs to be added to the app.</param>
    /// <response code="201">Client created.</response>.
    /// <response code="400">Client request not valid.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// Create a new client for the app with the specified name.
    /// The client secret is auto generated on the server and returned. The client does not expire, the access token is valid for 30 days.
    /// </remarks>
    [HttpPost]
    [Route("apps/{app}/clients/")]
    [ProducesResponseType(typeof(ClientsDto), 201)]
    [ApiPermissionOrAnonymous(PermissionIds.AppClientsCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostClient(string app, [FromBody] CreateClientDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetClients), new { app }, response);
    }

    /// <summary>
    /// Updates an app client.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the client that must be updated.</param>
    /// <param name="request">Client object that needs to be updated.</param>
    /// <response code="200">Client updated.</response>.
    /// <response code="400">Client request not valid.</response>.
    /// <response code="404">Client or app not found.</response>.
    /// <remarks>
    /// Only the display name can be changed, create a new client if necessary.
    /// </remarks>
    [HttpPut]
    [Route("apps/{app}/clients/{id}/")]
    [ProducesResponseType(typeof(ClientsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppClientsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutClient(string app, string id, [FromBody] UpdateClientDto request)
    {
        var command = request.ToCommand(id);

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Revoke an app client.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the client that must be deleted.</param>
    /// <response code="200">Client deleted.</response>.
    /// <response code="404">Client or app not found.</response>.
    /// <remarks>
    /// The application that uses this client credentials cannot access the API after it has been revoked.
    /// </remarks>
    [HttpDelete]
    [Route("apps/{app}/clients/{id}/")]
    [ProducesResponseType(typeof(ClientsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppClientsDelete)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteClient(string app, string id)
    {
        var command = new RevokeClient { Id = id };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    private async Task<ClientsDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<IAppEntity>();
        var response = GetResponse(result);

        return response;
    }

    private ClientsDto GetResponse(IAppEntity app)
    {
        return ClientsDto.FromApp(app, Resources);
    }
}
