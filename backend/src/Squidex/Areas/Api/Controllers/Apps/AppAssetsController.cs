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
public sealed class AppAssetsController : ApiController
{
    public AppAssetsController(ICommandBus commandBus)
        : base(commandBus)
    {
    }

    /// <summary>
    /// Get the app asset scripts.
    /// </summary>
    /// <param name="app">The name of the app to get the asset scripts for.</param>
    /// <response code="200">Asset scripts returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/assets/scripts")]
    [ProducesResponseType(typeof(AssetScriptsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetSScriptsRead)]
    [ApiCosts(0)]
    public IActionResult GetAssetScripts(string app)
    {
        var response = Deferred.Response(() =>
        {
            return GetResponse(App);
        });

        return Ok(response);
    }

    /// <summary>
    /// Update the app asset scripts.
    /// </summary>
    /// <param name="app">The name of the app to update.</param>
    /// <param name="request">The values to update.</param>
    /// <response code="200">Asset scripts updated.</response>.
    /// <response code="400">Asset request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPut]
    [Route("apps/{app}/assets/scripts")]
    [ProducesResponseType(typeof(AssetScriptsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsScriptsUpdate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutAssetScripts(string app, [FromBody] UpdateAssetScriptsDto request)
    {
        var response = await InvokeCommandAsync(request.ToCommand());

        return Ok(response);
    }

    private async Task<AssetScriptsDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<IAppEntity>();
        var response = GetResponse(result);

        return response;
    }

    private AssetScriptsDto GetResponse(IAppEntity result)
    {
        return AssetScriptsDto.FromDomain(result, Resources);
    }
}
