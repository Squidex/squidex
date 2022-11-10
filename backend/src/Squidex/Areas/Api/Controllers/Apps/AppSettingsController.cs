// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Apps))]
public sealed class AppSettingsController : ApiController
{
    public AppSettingsController(ICommandBus commandBus)
        : base(commandBus)
    {
    }

    /// <summary>
    /// Get the app settings.
    /// </summary>
    /// <param name="app">The name of the app to get the settings for.</param>
    /// <response code="200">App settings returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/settings")]
    [ProducesResponseType(typeof(AppSettingsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(0)]
    public IActionResult GetSettings(string app)
    {
        var response = Deferred.Response(() =>
        {
            return GetResponse(App);
        });

        return Ok(response);
    }

    /// <summary>
    /// Update the app settings.
    /// </summary>
    /// <param name="app">The name of the app to update.</param>
    /// <param name="request">The values to update.</param>
    /// <response code="200">App updated.</response>.
    /// <response code="400">App request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPut]
    [Route("apps/{app}/settings")]
    [ProducesResponseType(typeof(AppSettingsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppUpdateSettings)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutSettings(string app, [FromBody] UpdateAppSettingsDto request)
    {
        var response = await InvokeCommandAsync(request.ToCommand());

        return Ok(response);
    }

    private async Task<AppSettingsDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<IAppEntity>();
        var response = GetResponse(result);

        return response;
    }

    private AppSettingsDto GetResponse(IAppEntity result)
    {
        return AppSettingsDto.FromDomain(result, Resources);
    }
}
