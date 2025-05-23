﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Apps))]
public sealed class AppRolesController(ICommandBus commandBus, RolePermissionsProvider permissionsProvider) : ApiController(commandBus)
{
    /// <summary>
    /// Get app roles.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Roles returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/roles/")]
    [ProducesResponseType(typeof(RolesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRolesRead)]
    [ApiCosts(0)]
    public IActionResult GetRoles(string app)
    {
        var response = Deferred.Response(() =>
        {
            return GetResponse(App);
        });

        Response.Headers[HeaderNames.ETag] = App.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Get app permissions.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">App permissions returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/roles/permissions")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRolesRead)]
    [ApiCosts(0)]
    public IActionResult GetPermissions(string app)
    {
        var response = Deferred.AsyncResponse(() =>
        {
            return permissionsProvider.GetPermissionsAsync(App);
        });

        Response.Headers[HeaderNames.ETag] = string.Concat(response).ToSha256Base64();

        return Ok(response);
    }

    /// <summary>
    /// Add role to app.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">Role object that needs to be added to the app.</param>
    /// <response code="201">Role created.</response>
    /// <response code="400">Role request not valid.</response>
    /// <response code="404">App not found.</response>
    [HttpPost]
    [Route("apps/{app}/roles/")]
    [ProducesResponseType(typeof(RolesDto), StatusCodes.Status201Created)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRolesCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostRole(string app, [FromBody] AddRoleDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetRoles), new { app }, response);
    }

    /// <summary>
    /// Update an app role.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="roleName">The name of the role to be updated.</param>
    /// <param name="request">Role to be updated for the app.</param>
    /// <response code="200">Role updated.</response>
    /// <response code="400">Role request not valid.</response>
    /// <response code="404">Role or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/roles/{roleName}/")]
    [ProducesResponseType(typeof(RolesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRolesUpdate)]
    [ApiCosts(1)]
    [UrlDecodeRouteParams]
    public async Task<IActionResult> PutRole(string app, string roleName, [FromBody] UpdateRoleDto request)
    {
        var command = request.ToCommand(roleName);

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Remove role from app.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="roleName">The name of the role.</param>
    /// <response code="200">Role deleted.</response>
    /// <response code="400">Role is in use by contributor or client or a default role.</response>
    /// <response code="404">Role or app not found.</response>
    [HttpDelete]
    [Route("apps/{app}/roles/{roleName}/")]
    [ProducesResponseType(typeof(RolesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRolesDelete)]
    [ApiCosts(1)]
    [UrlDecodeRouteParams]
    public async Task<IActionResult> DeleteRole(string app, string roleName)
    {
        var command = new DeleteRole { Name = roleName };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    private async Task<RolesDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<App>();
        var response = GetResponse(result);

        return response;
    }

    private RolesDto GetResponse(App result)
    {
        return RolesDto.FromDomain(result, Resources);
    }
}
